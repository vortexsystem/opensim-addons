using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Nini.Config;
using Mono.Addins;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using Caps = OpenSim.Framework.Capabilities.Caps;

[assembly: Addin("ServerReleaseNotes", "0.1")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]

namespace Never.Modules
{
    public interface IServerReleaseNotes
    {
        //not used yet
    }

    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule")]


    public class ServerReleaseNotes : IServerReleaseNotes, ISharedRegionModule
    {

        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, Scene> m_Scenes = new Dictionary<string, Scene>();
        private bool m_enabled;
        #region properties

        public string m_name = "ServerReleaseNotes";

        #endregion properties

        private Scene m_scene;
        private static string m_ServerReleaseNotesURL = string.Empty;


        #region ISharedRegionModule implementation

        public void PostInitialise()
        {
            //Not implemented
        }

        #endregion

        #region IRegionModuleBase implementation

        public void Initialise(IConfigSource source)
        {

            IConfig ServerReleaseNote = source.Configs["ServerReleaseNotesURL"];
            m_ServerReleaseNotesURL = ServerReleaseNote.GetString("ServerReleaseNotesURL", m_ServerReleaseNotesURL);
            m_enabled = ServerReleaseNote.GetBoolean("enabled", false);

            if (m_ServerReleaseNotesURL == null)
            {
                m_enabled = false;
                m_log.Info("[ServerReleaseNotes]: No Configuration Found, Disabled");
                return;
            }


            if (m_enabled == false)
            {
                m_log.InfoFormat("[ServerReleaseNote]: Module is disabled");
                return;
            }

        }

        public void AddRegion(Scene scene)
        {
            if (!m_enabled)
                return;

            m_scene = scene;

            if (m_enabled == true)
            {
                if (m_Scenes.ContainsKey(scene.RegionInfo.RegionName))
                {
                    lock (m_Scenes)
                    {
                        m_Scenes[scene.RegionInfo.RegionName] = scene;
                    }
                }
            }
            else
            {
                lock (m_Scenes)
                {
                    m_Scenes.Add(scene.RegionInfo.RegionName, scene);
                }
            }

            m_scene.EventManager.OnRegisterCaps += RegisterCaps;

        }

        public void RemoveRegion(Scene scene)
        {
            if (!m_enabled)
                return;

            m_scene.EventManager.OnRegisterCaps -= RegisterCaps;
        }

        public void RegionLoaded(Scene scene)
        {
            if (!m_enabled)
                return;

        }

        #endregion

        public void Close()
        {
            if (!m_enabled)
                return;
        }
        public string Name { get { return "ServerReleaseNotes"; } }

        public Type ReplaceableInterface
        {
            get { return null; }
        }
        public void RegisterCaps(UUID agentID, Caps caps)
        {

            UUID capuuid = UUID.Random();


            IRequestHandler ServerReleaseNote
                = new RestHTTPHandler(
                    "GET", "/CAPS/ServerReleaseNotes/" + capuuid + "/",
                       delegate(Hashtable m_dhttpMethod)
                       { return ProcessServerReleaseNotes(m_dhttpMethod, agentID, capuuid); });

            caps.RegisterHandler("ServerReleaseNotes", ServerReleaseNote);

        }


        private Hashtable ProcessServerReleaseNotes(Hashtable m_dhttpMethod, UUID agentID, UUID capuuid)
        {
            Hashtable responsedata = new Hashtable();
            responsedata["int_response_code"] = 301;
            responsedata["str_redirect_location"] = m_ServerReleaseNotesURL;
            responsedata["content_type"] = "text/plain";
            responsedata["keepalive"] = false;

            OSDMap osd = new OSDMap();
            osd.Add("ServerReleaseNotes", new OSDString(GetServerReleaseNotesURL()));

            string response = OSDParser.SerializeLLSDXmlString(osd);
            responsedata["str_response_string"] = response;

            return responsedata;
        }


        public static string GetServerReleaseNotesURL()
        {
            return "Set Release Notes Url in OpenSim.ini under [ServerReleaseNotesURL] section";
        }

    }
}
