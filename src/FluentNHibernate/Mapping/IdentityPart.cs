using System;
using System.Reflection;
using System.Xml;

namespace FluentNHibernate.Mapping
{
    public class IdentityPart : IIdentityPart
    {
		private string _columnName;
		private readonly IdentityGenerationStrategyBuilder _generatedBy;
		private readonly Cache<string, string> _generatorParameters = new Cache<string, string>();
        private readonly Cache<string, string> _elementAttributes = new Cache<string, string>();
		private readonly PropertyInfo _property;
		private string _generatorClass;
        private readonly AccessStrategyBuilder<IIdentityPart> access;
	    private object _unsavedValue;

	    public IdentityPart(PropertyInfo property, string columnName)
		{
            access = new AccessStrategyBuilder<IIdentityPart>(this);

			_property = property;
			_columnName = columnName;
			_generatedBy = new IdentityGenerationStrategyBuilder(this);
		}

		public IdentityPart(PropertyInfo property) : this(property, null)
		{
		}

		public IdentityGenerationStrategyBuilder GeneratedBy
		{
			get { return _generatedBy; }
		}

		private string generatorClass
		{
			get
			{
				if (_generatorClass != null) return _generatorClass;
				if (IdentityType == typeof (Guid)) return "guid.comb";
				if (IdentityType == typeof (int) || IdentityType == typeof (long))
					return "identity";
				return "assigned";
			}
		}

		public Type IdentityType
		{
			get { return _property.PropertyType; }
		}

		public void Write(XmlElement classElement, IMappingVisitor visitor)
		{
            visitor.Conventions.AlterId(this);

            var columnName = (String.IsNullOrEmpty(_columnName))
             ? visitor.Conventions.CalculatePrimaryKey(_property)
             : _columnName;

			XmlElement element = classElement.AddElement("id")
				.WithAtt("name", _property.Name)
                .WithAtt("column", columnName)
				.WithAtt("type", TypeMapping.GetTypeString(_property.PropertyType));

            if (_unsavedValue != null)
				element.WithAtt("unsaved-value", _unsavedValue.ToString());

            _elementAttributes.ForEachPair((name, value) => element.WithAtt(name, value));

			XmlElement generatorElement = element.AddElement("generator").WithAtt("class", generatorClass);
			_generatorParameters.ForEachPair(
				(name, innerXml) => generatorElement.AddElement("param").WithAtt("name", name).InnerXml = innerXml);
		}

        /// <summary>
        /// Set an attribute on the xml element produced by this identity mapping.
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="value">Attribute value</param>
	    public void SetAttribute(string name, string value)
	    {
	        _elementAttributes.Store(name, value);
	    }

        public void SetAttributes(Attributes atts)
        {
            foreach (var key in atts.Keys)
            {
                SetAttribute(key, atts[key]);
            }
        }

	    public int Level
		{
			get { return 0; }
		}

	    public PartPosition Position
	    {
            get { return PartPosition.First; }
	    }

        public IIdentityPart SetGeneratorClass(string generator)
		{
			_generatorClass = generator;
			return this;
		}

        public IIdentityPart AddGeneratorParam(string name, string innerXml)
		{
			_generatorParameters.Store(name, innerXml);
			return this;
		}

        /// <summary>
        /// Set the access and naming strategy for this identity.
        /// </summary>
        public AccessStrategyBuilder<IIdentityPart> Access
	    {
	        get { return access; }
	    }

        /// <summary>
        /// Sets the unsaved-value of the identity.
        /// </summary>
        /// <param name="unsavedValue">Value that represents an unsaved value.</param>
        public IIdentityPart WithUnsavedValue(object unsavedValue)
        {
            _unsavedValue = unsavedValue;
            return this;
        }
	}
}
