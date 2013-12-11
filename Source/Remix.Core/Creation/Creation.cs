namespace Atlana.Creation
{
    using System;
    using System.Xml;
    using System.Linq;
    using System.Collections.Generic;
    using Atlana.Configuration;
    using Atlana.Network;
    using System.Text;

    public enum CreationUI
    {
        Text = 0
    }
    
    public struct CreationOption
    {
        public string Property;
        public CreationUI UI;
        public DescriptorStates State;
        public DescriptorStates NextState;
        public DescriptorStates FailState;
        public string Prompt;
        public bool Confirm;
        public bool Encrypt;
        public string FailMessage;
    }
    
    /// <summary>
    /// Description of Creation.
    /// </summary>
    public sealed class CreationManager
    {
        private static CreationManager instance = new CreationManager();

        public static CreationManager Instance
        {
            get
            {
                return instance;
            }
        }
        
        private List<CreationOption> Options;
        
        public int Count
        {
            get
            {
                return this.Options.Count;
            }
        }
        
        private CreationManager()
        {
            Options=new List<CreationOption>();
        }

        public bool LoadOptions()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(string.Format("{0}\\{1}", SettingsManager.SystemDirectory, SettingsManager.CreationFile));
                foreach (XmlNode n in xml.SelectNodes("/options/opt"))
                {
                    CreationOption c = new CreationOption();
                    foreach (XmlAttribute a in n.Attributes)
                    {
                        switch (a.Name)
                        {
                            case "property":
                                c.Property = a.Value;
                                break;
                            case "failmsg":
                                c.FailMessage = a.Value;
                                break;
                            case "ui":
                                c.UI = (CreationUI)Enum.Parse(typeof(CreationUI), a.Value);
                                break;
                            case "state":
                                c.State = (DescriptorStates)Enum.Parse(typeof(DescriptorStates), a.Value);
                                break;
                            case "next":
                                c.NextState = (DescriptorStates)Enum.Parse(typeof(DescriptorStates), a.Value);
                                break;
                            case "fail":
                                c.FailState = (DescriptorStates)Enum.Parse(typeof(DescriptorStates), a.Value);
                                break;
                            case "encrypt":
                                c.Encrypt = (a.Value == bool.TrueString) ? true : false;
                                break;
                            case "confirm":
                                c.Confirm = (a.Value == bool.TrueString) ? true : false;
                                break;
                        }
                    }

                    if (n.InnerText != null && n.InnerText != "")
                    {
                        c.Prompt = n.InnerText;
                    }

                    this.Options.Add(c);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Bug("CreationManager.LoadOptions: {0}", e.Message);
                return false;
            }

            return true;
        }

        public void PromptOption(ClientDescriptor d)
        {
            if (d.State == DescriptorStates.PressEnter)
            {
                d.Player.WriteLine("Press Enter...");
            }
            else
            {
                if (this.Options.AsQueryable().Count(z => z.State == d.State) <= 0)
                {
                    return;
                }

                CreationOption c = this.Options.First(z => z.State == d.State);
                if (c.State != d.State)
                {
                    return;
                }

                d.Player.Write(c.Prompt);
            }
        }

        public void Process(ClientDescriptor d, string arg)
        {
            string s = arg;
            if (d.Player == null)
            {
                d.Player = new Player();
            }

            foreach (CreationOption c in this.Options)
            {
                if (c.State == d.State)
                {
                    if (c.Encrypt && s != "")
                    {
                        s = Convert.ToBase64String(Encoding.ASCII.GetBytes(s));
                    }

                    if (c.Confirm)
                    {
                        if (s != (string)d.Player.GetProperty(c.Property))
                        {
                            d.State = c.FailState;
                            d.Player.WriteLine(c.FailMessage);
                        }
                        else
                        {
                            d.State = c.NextState;
                        }
                    }
                    else
                    {
                        switch (c.UI)
                        {
                            default:
                                d.Player.SetProperty(c.Property, s);
                                break;
                            case CreationUI.Text:
                                d.Player.SetProperty(c.Property, s);
                                break;
                        }

                        d.State = c.NextState;
                    }

                    this.PromptOption(d);
                    return;
                }
            }
        }
    }
}
