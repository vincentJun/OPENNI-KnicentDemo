using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using OpenNI;

namespace KinectServer
{
    public class PlayerController : ApiController
    {
        [HttpGet, Route("getposition")]
        public Point3D GetPosition()
        {
            return KinectManager.Instance.GetPlayerPosition();
        }

        [HttpGet, Route("getfire")]
        public bool GetFire()
        {
            return KinectManager.Instance.GetFire();
        }
    }
}
