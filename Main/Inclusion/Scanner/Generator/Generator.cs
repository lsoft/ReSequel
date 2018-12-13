using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Inclusion.Scanner.Generator
{
    public class Generator
    {
        //private readonly Dictionary<string, uint> _indexes;

        private readonly Dictionary<string, (int Count, List<InternalOption> Options)> _options = new Dictionary<string, (int Count, List<InternalOption> Options)>();

        private string _queryTemplate;

        private int _optionCount = 0;

        public string SqlTemplate
        {
            get
            {
                return _queryTemplate;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return
                    string.IsNullOrWhiteSpace(_queryTemplate) || _options.Count == 0;
            }
        }

        public Generator(
            )
        {
        }

        public void WithQuery(
            string queryTemplate
            )
        {
            _queryTemplate = queryTemplate;
        }

        public void BindToOption(
            params string[] args
            )
        {
            BindToOption(args.ToList());
        }

        public void BindToOption(
            List<string> args
            )
        {
            if (args == null || args.Count == 0)
            {
                throw new ArgumentException(nameof(args));
            }

            var optionName = args[0];
            var parts = args.Skip(1).ToArray();

            (int Count, List<InternalOption> Options) existeds;
            if (_options.TryGetValue(optionName, out existeds))
            {
                if (existeds.Options.Any(j => j.Parts.Length != parts.Length))
                {
                    throw new InvalidOperationException("parts length is unequals");
                }
            }

            var option = new InternalOption(
                _optionCount,
                optionName,
                parts
                );

            if (!_options.ContainsKey(optionName))
            {
                _options.Add(optionName, (parts.Length, new List<InternalOption>()));
            }

            _options[optionName].Options.Add(option);

            _optionCount++;
        }

        public IEnumerable<string> FormattedQueries
        {
            get
            {
                List<List<int>> lists = new List<List<int>>();

                foreach (var pair in _options)
                {
                    lists.Add(
                        Enumerable.Range(0, pair.Value.Count).ToList()
                        );
                }

                IEnumerable<List<int>> joinEquation = lists.ElementAt(0).ConvertAll(j => new List<int> { j });

                if (lists.Count > 1)
                {
                    foreach (var list in lists.Skip(1))
                    {
                        joinEquation = joinEquation
                            .Join(
                                list.ConvertAll(j => new List<int> { j }),
                                i => 1,
                                i => 1,
                                (i0, i1) => i0.Concat(i1).ToList()
                                )
                            ;
                    }
                }

                var valueList = _options.Values.ToList();

                foreach (var indexes in joinEquation)
                {
                    var preformat = new List<(int FormatIndex, string OptionValue)>();

                    for (var cc = 0; cc < valueList.Count; cc++)
                    {
                        var index = indexes[cc];
                        var optionList = valueList[cc].Options;

                        foreach (var option in optionList)
                        {
                            preformat.Add((option.SequenceIndex, option.Parts[index]));
                        }
                    }

                    var result = string.Format(
                        _queryTemplate,
                        preformat.OrderBy(j => j.FormatIndex).Select(j => j.OptionValue).Cast<object>().ToArray()
                        );

                    yield return result;
                }
            }
        }

    }
}
