namespace Atlana.Help
{
    using System;
    using System.Xml;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using Atlana.Configuration;
    using Atlana.Log;
    using System.Text;
    using Atlana.Data;

    /// <summary>
    /// Description of HelpManager.
    /// </summary>
    public sealed class HelpManager
    {
        private static HelpManager instance = new HelpManager();

        public static HelpManager Instance
        {
            get
            {
                return instance;
            }
        }

        public int CatalogCount
        {
            get
            {
                string lc = null;
                int c = 0;
                foreach (HelpArticle h in this.Articles)
                {
                    if (lc == null || h.Category != lc)
                    {
                        lc = h.Category;
                        c++;
                    }
                }

                return c;
            }
        }

        public int ArticleCount
        {
            get
            {
                return this.Articles.Count;
            }
        }

        private List<HelpArticle> Articles;

        private HelpManager()
        {
            this.Articles = new List<HelpArticle>();
        }

        public HelpArticle[] GetHelps(string category)
        {
            if (this.Articles.FirstOrDefault(z => z.Category == category) != null)
            {
                return this.Articles.Where(z => z.Category == category).ToArray();
            }

            return null;
        }

        public HelpArticle GetHelp(string name)
        {
            return this.Articles.FirstOrDefault(z => z.Name == name);
        }

        public bool LoadArticles()
        {
            bool ok = true;
            try
            {
                using (var helpRepo = new Repository<HelpArticle>())
                {
                    this.Articles.AddRange(helpRepo.All());
                }
            }
            catch (Exception e)
            {
                Logger.Bug("LoadArticles:{0}", e.Message);
                ok = false;
            }

            return ok;
        }
    }
}
