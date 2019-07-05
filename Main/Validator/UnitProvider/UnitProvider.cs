using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;
using Main.Validator.UnitProvider.Bag;

namespace Main.Validator.UnitProvider
{
    public class UnitProvider : IUnitProvider
    {
        private readonly Func<bool> _shouldBreak;

        private readonly Dictionary<IValidatedSqlInclusion, IInclusionBag> _artifacts = new Dictionary<IValidatedSqlInclusion, IInclusionBag>();

        private readonly object _locker = new object();

        private readonly IEnumerator<IValidationUnit> _unitEnumerator;

        public IReadOnlyDictionary<IValidatedSqlInclusion, IInclusionBag> Artifacts => _artifacts;

        /// <summary>
        /// Total count of variants that should be checked
        /// </summary>
        public int TotalVariantCount
        {
            get;
        }

        /// <summary>
        /// Checked variant count
        /// </summary>
        public int CheckedVariantCount
        {
            get;
            private set;
        }

        public UnitProvider(
            List<IValidatedSqlInclusion> inclusions,
            Func<bool> shouldBreak
            )
        {
            if (inclusions == null)
            {
                throw new ArgumentNullException(nameof(inclusions));
            }

            if (shouldBreak == null)
            {
                throw new ArgumentNullException(nameof(shouldBreak));
            }

            _shouldBreak = shouldBreak;

            if (inclusions.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inclusions));
            }

            TotalVariantCount = inclusions.Sum(inclusion => inclusion.Inclusion.FormattedQueriesCount);

            foreach (var inclusion in inclusions)
            {
                var bag = new InclusionBag(inclusion);
                _artifacts[inclusion] = bag;
            }

            _unitEnumerator = RequestNextUnitInternal().GetEnumerator();
        }


        public bool TryRequestNextUnit(out IValidationUnit unit)
        {
            lock (_locker)
            {
                if (_unitEnumerator.MoveNext())
                {
                    CheckedVariantCount++;

                    unit = _unitEnumerator.Current;
                    return true;
                }

                unit = null;
                return false;
            }
        }

        public IEnumerable<IValidationUnit> RequestNextUnitSync()
        {
            while (true)
            {
                IValidationUnit current = null;
                lock (_locker)
                {
                    if (!_unitEnumerator.MoveNext())
                    {
                        yield break;
                    }

                    CheckedVariantCount++;

                    current = _unitEnumerator.Current;
                }

                yield return current;
            }
        }

        private IEnumerable<IValidationUnit> RequestNextUnitInternal()
        {
            foreach (var pair in _artifacts)
            {
                var bag = pair.Value;
                var inclusion = bag.Inclusion;

                foreach (var sqlBody in inclusion.Inclusion.FormattedSqlBodies)
                {
                    if (_shouldBreak())
                    {
                        yield break;
                    }

                    if (bag?.Result?.IsFailed ?? false)
                    {
                        //one of the variants of the generator has failed.
                        //so there is no need to continue validate that generator
                        break;
                    }

                    var unit = new ValidationUnit(bag, sqlBody);
                    yield return unit;
                }
            }
        }
    }

}
