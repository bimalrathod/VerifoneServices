
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Reflection;

namespace CustomAction
{
    [RunInstaller(true)]
    public class MycustomAction : Installer 
    {
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]


      
        public override void Install(IDictionary savedState)
        {
            //System.Diagnostics.Debugger.Launch();
            base.Install(savedState);

            string installationPath = this.Context.Parameters["targetdir"];
            string VL = Context.Parameters["VL"];
            string VU = Context.Parameters["VU"];
            string VP = Context.Parameters["VP"];
            string ApplicationKey = Context.Parameters["ApplicationKey"];

            // above for rapid sever 
            //string DS = "RJKDACEK5BAQYT3LBXTHZZTZNAC3C7RH5RGF98UAUL5862V7PZQA";
            //string FN = "FRYLKDDY93PKM2BRZTEJFKYLZX";
            //string PN = "837JK5QVK4DLVDLBQTK8FYAZAW";

            // above for rapid verfrione server
            string DS = "QMXGHJAUJJKNBR49C7ZBACS6ZSSEFKEN4YUUMRFB6GHUS8TYP3ZFKGNU3KW2NBXJ9DGBWE93QYU5V";
            string FN = "8FZUD6M4XFQDYAMS9TC35DJ7Q3";
            string PN = "837JK5QVK4DLVDLBQTK8FYAZAW";



            //string DS = "", FN = "", PN = "";
            ////string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataSource\\Datavariables.xml");
            //string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"DataSource\Datavariables.xml");
            //XDocument xdoc = XDocument.Load(path);

            //XElement dbServers = xdoc.Element("VerifoneServers");


            //var dbDetailResult = (from dbDetail in dbServers.Elements("RapidrmsServers")
            //                      from x in dbDetail.Elements("RapidrmsServer")
            //                      select new
            //                      {
            //                          DbId = Convert.ToInt32(x.Attribute("id").Value),
            //                          DS = x.Attribute("DS").Value,
            //                          FN = x.Attribute("FN").Value,
            //                          PN = x.Attribute("PN").Value
            //                      }).ToList()[0];

            XDocument doc =
                      new XDocument(
                        new XElement("VerifoneServers",
                          new XElement("VerifoneServer", new XAttribute("id", "1"), new XAttribute("VL", VL.Encript()), new XAttribute("VU", VU.Encript())
                              , new XAttribute("VP", VP.Encript()), new XAttribute("ApplicationKey", ApplicationKey)),
                          new XElement("RapidrmsServers",
                              new XElement("RapidrmsServer", new XAttribute("id", "1"), new XAttribute("DS", DS), new XAttribute("FN", FN),
                                  new XAttribute("PN", PN))
                          )));


            var currentDirectory = Path.Combine(installationPath, "DataSource");
            bool IsExists = System.IO.Directory.Exists(currentDirectory);
            if (!IsExists) System.IO.Directory.CreateDirectory(currentDirectory);

            var fileName = Path.Combine(currentDirectory, "DatabaseServers.xml");
            doc.Save(fileName);

        }


       
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);            
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }
    }
}
