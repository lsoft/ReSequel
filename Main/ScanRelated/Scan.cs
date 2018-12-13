using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class Scan
    {
        private readonly object _locker = new object();
        private volatile IDictionary<string, ScanProjectContainer> _containerDictionary;
        private volatile IDictionary<string, ScanProjectGenerator> _generatorDictionary;

        [XmlElement(ElementName = "Project")]
        public ScanProject[] Projects
        {
            get;
            set;
        }

        public IDictionary<string, ScanProjectContainer> ContainerDictionary
        {
            get
            {
                ConstructAll();

                return
                    _containerDictionary;
            }
        }

        public IDictionary<string, ScanProjectGenerator> GeneratorDictionary
        {
            get
            {
                ConstructAll();

                return
                    _generatorDictionary;
            }
        }

        public IReadOnlyCollection<string> GetAllContainerProperties()
        {
            if (Projects == null)
            {
                return
                    new HashSet<string>();
            }

            return
                (from project in Projects
                 where project != null && project.Containers != null
                 from @class in project.Containers
                 where @class != null && @class.Properties != null
                 from property in @class.Properties
                 where property != null && !string.IsNullOrEmpty(property.Name)
                 select property.Name).Distinct().ToHashSet()
                ;
        }

        //public IReadOnlyCollection<string> GetAllGeneratorTypes()
        //{
        //    if (Projects == null)
        //    {
        //        return
        //            new HashSet<string>();
        //    }

        //    return
        //        (from project in Projects
        //         where project != null && project.Generators != null
        //         from @class in project.Generators
        //         where @class != null && !string.IsNullOrEmpty(@class.Name)
        //         select @class.Name).Distinct().ToHashSet()
        //        ;
        //}


        public IReadOnlyCollection<string> GetAllGeneratorMethods()
        {
            if (Projects == null)
            {
                return
                    new HashSet<string>();
            }

            return
                (from project in Projects
                 where project != null && project.Generators != null
                 from @class in project.Generators
                 where @class != null && @class.Methods != null
                 from method in @class.Methods
                 where method != null && !string.IsNullOrEmpty(method.Name)
                 select method.Name).Distinct().ToHashSet()
                ;
        }

        public IReadOnlyCollection<string> GetAllContainerMethods()
        {
            if (Projects == null)
            {
                return
                    new HashSet<string>();
            }

            return
                (from project in Projects
                 where project != null && project.Containers != null
                 from @class in project.Containers
                 where @class != null && @class.Methods != null
                 from method in @class.Methods
                 where method != null && !string.IsNullOrEmpty(method.Name)
                 select method.Name).Distinct().ToHashSet()
                ;
        }

        private void ConstructAll()
        {
            if (_containerDictionary == null)
            {
                lock (_locker)
                {
                    if(_containerDictionary==null)
                    {
                        if(Projects==null)
                        {
                            _containerDictionary = new Dictionary<string, ScanProjectContainer>();
                            _generatorDictionary = new Dictionary<string, ScanProjectGenerator>();
                            return;
                        }
                        _generatorDictionary = (
                            from project in Projects
                            where project != null && project.Generators != null
                            from @class in project.Generators
                            where @class != null && !string.IsNullOrEmpty(@class.Name)
                            select @class
                            ).ToDictionary(j => j.Name, k => k);


                        _containerDictionary = (
                            from project in Projects
                            where project != null && project.Containers != null
                            from @class in project.Containers
                            where @class != null && !string.IsNullOrEmpty(@class.Name)
                            select @class
                            ).ToDictionary(j => j.Name, k => k);
                    }
                }
            }
        }
    }
}
