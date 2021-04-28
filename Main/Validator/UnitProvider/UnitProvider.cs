using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;
using Main.Progress;
using Main.Validator.UnitProvider.Bag;

namespace Main.Validator.UnitProvider
{
    public class UnitProvider : IUnitProvider
    {
        private readonly Func<bool> _shouldBreak;
        private readonly ValidationProgress _progress;

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
            Func<bool> shouldBreak, 
            ValidationProgress progress
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

            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            _shouldBreak = shouldBreak;
            _progress = progress;

            if (inclusions.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inclusions));
            }

            TotalVariantCount = inclusions.Sum(inclusion => inclusion.Inclusion.FormattedQueriesCount);

            foreach (var inclusion in inclusions.OrderBy(j => j.Inclusion.FormattedQueriesCount)) //put heaviest generators at the end of the list
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

                inclusion.SetStatusInProgress(0, inclusion.Inclusion.FormattedQueriesCount);

                var enume = inclusion.Inclusion.FormattedSqlBodies.GetEnumerator();
                try
                {
                    while (true)
                    {
                        try
                        {
                            if (!enume.MoveNext())
                            {
                                break;
                            }
                        }
                        catch (Exception excp)
                        {
                            Debug.WriteLine(excp.Message);
                            Debug.WriteLine(excp.StackTrace);

                            bag.SetValidationResult(
                                new ExceptionValidationResult(
                                    inclusion.Inclusion.SqlBody,
                                    excp
                                    )
                                );

                            //so there is no need to continue validate that generator
                            break;
                        }


                        //string sqlBody;
                        //try
                        //{
                        //    sqlBody = enume.Current;
                        //}
                        //catch (Exception excp)
                        //{
                        //    Debug.WriteLine(excp.Message);
                        //    Debug.WriteLine(excp.StackTrace);

                        //    bag.SetValidationResult(
                        //        new ExceptionValidationResult(
                        //            inclusion.Inclusion.SqlBody,
                        //            excp
                        //            )
                        //        );

                        //    //so there is no need to continue validate that generator
                        //    break;
                        //}
                        var sqlBody = enume.Current;

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
                finally
                {
                    enume.Dispose();
                }

                //foreach (var sqlBody in inclusion.Inclusion.FormattedSqlBodies)
                //{
                //    if (_shouldBreak())
                //    {
                //        yield break;
                //    }

                //    if (bag?.Result?.IsFailed ?? false)
                //    {
                //        //one of the variants of the generator has failed.
                //        //so there is no need to continue validate that generator
                //        break;
                //    }

                //    var unit = new ValidationUnit(bag, sqlBody);
                //    yield return unit;
                //}

                _progress.AddProcessedInclusionCount(1);
            }
        }
    }

}
