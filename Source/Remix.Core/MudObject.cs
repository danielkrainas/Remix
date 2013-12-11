namespace Atlana
{
    using System.Dynamic;

    public abstract class MudObject
    {
        private ExpandoObject extendedProperties = new ExpandoObject();

        public ExpandoObject ExtendedProperties
        {
            get
            {
                return this.extendedProperties;
            }
        }
    }
}
