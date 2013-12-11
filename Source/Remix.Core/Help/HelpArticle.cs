namespace Atlana.Help
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;

    public class HelpArticle
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Category
        {
            get;
            set;
        }

        public short MinLevel
        {
            get;
            set;
        }

        public short MaxLevel
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        [NotMapped]
        public HelpIndexModes Mode
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", this.Category, this.Name);
        }
    }
}
