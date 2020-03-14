namespace Unity.Properties
{
    struct FormerlySerializedAsVisitor : IPropertyVisitor
    {
        readonly string m_SerializedName;
        public string CurrentName;
        
        public FormerlySerializedAsVisitor(string serializedName)
        {
            m_SerializedName = serializedName;
            CurrentName = string.Empty;
        }
        
        public VisitStatus VisitProperty<TProperty, TContainer, TValue>(
            TProperty property,
            ref TContainer container,
            ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, TValue>
            => Visit<TProperty, TContainer, TValue>(property);

        public VisitStatus VisitCollectionProperty<TProperty, TContainer, TValue>(
            TProperty property,
            ref TContainer container,
            ref ChangeTracker changeTracker)
            where TProperty : ICollectionProperty<TContainer, TValue>
            => Visit<TProperty, TContainer, TValue>(property);
        
        VisitStatus Visit<TProperty, TContainer, TValue>(
            TProperty property)
            where TProperty : IProperty<TContainer, TValue>
        {
            if (null == property.Attributes)
            {
                return VisitStatus.Override;
            }

            var ourFormerlySerializedAsAttributes = property.Attributes.GetAttributes<FormerlySerializedAsAttribute>();
            if (null != ourFormerlySerializedAsAttributes)
            {
                foreach (var formerlySerializedAs in ourFormerlySerializedAsAttributes)
                {
                    if (formerlySerializedAs.OldName != m_SerializedName)
                    {
                        continue;
                    }
                    CurrentName = property.GetName();
                    return VisitStatus.Override;
                }
            }

            var theirFormerlySerializedAsAttributes = property.Attributes.GetAttributes<UnityEngine.Serialization.FormerlySerializedAsAttribute>();
            if (null != theirFormerlySerializedAsAttributes)
            {
                foreach (var formerlySerializedAs in theirFormerlySerializedAsAttributes)
                {
                    if (formerlySerializedAs.oldName != m_SerializedName)
                    {
                        continue;
                    }
                    CurrentName = property.GetName();
                    return VisitStatus.Override;
                }
            }

            return VisitStatus.Override;
        }
    }
}