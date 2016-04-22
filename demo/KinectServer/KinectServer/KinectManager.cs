using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenNI;

namespace KinectServer
{
    public class KinectManager
    {
        private static KinectManager instance;
       

        public static KinectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KinectManager();
                }

                return instance;

            }
        }

        private readonly Context context;
        private readonly UserGenerator userGenerator;
        private readonly SkeletonCapability skeletonCapability;
        private readonly PoseDetectionCapability poseDetectionCapability;
        private readonly ScriptNode scriptNode;
        private readonly ConcurrentDictionary<int, Point3D> positions = new ConcurrentDictionary<int, Point3D>();
        private readonly ConcurrentDictionary<int, bool> fires = new ConcurrentDictionary<int, bool>();
        private readonly string calibPose;
        private readonly DepthGenerator depth;

        private int playerId = -1;

        private bool close = false;
        private KinectManager()
        {
            this.context = Context.CreateFromXmlFile("SamplesConfig.xml", out this.scriptNode);
            this.depth = context.FindExistingNode(NodeType.Depth) as DepthGenerator;
            this.userGenerator = new UserGenerator(context);
            this.userGenerator.NewUser += UserGenerator_NewUser;
            this.userGenerator.UserExit += UserGenerator_UserExit;
            this.skeletonCapability = this.userGenerator.SkeletonCapability;
            this.poseDetectionCapability = this.userGenerator.PoseDetectionCapability;
            this.poseDetectionCapability.PoseDetected += PoseDetectionCapability_PoseDetected;
            this.skeletonCapability.CalibrationComplete += SkeletonCapability_CalibrationComplete;
            this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);
            this.calibPose = this.skeletonCapability.CalibrationPose;
            this.poseDetectionCapability.PoseDetected += poseCap_PoseDetected;
            // this.userGenerator.StartGenerating();

            // this.context.StartGeneratingAll();
        }

        public event PlayerDetectedEventHandler OnPlayerDetected;
        public event PlayerDetectedEventHandler OnNewPlayer;
        public event PlayerDetectedEventHandler OnPlayerLost;
        // 开辟一个线程  调用Generator接口
        public void Start()
        {
            
            Task.Run(() =>
            {
                try
                {
                    this.userGenerator.StartGenerating();
                    while (!this.close)
                    {
                        this.context.WaitOneUpdateAll(this.userGenerator);
                        var users = this.userGenerator.GetUsers();
                        foreach (var user in users)
                        {
                            if (this.skeletonCapability.DoesNeedPoseForCalibration)
                            {
                                this.skeletonCapability.RequestCalibration(user, true);
                            }
                            else
                            {
                                if (this.skeletonCapability.IsTracking(user))
                                {
                                    Console.WriteLine("9999999999");
                                    this.EnqueueRaiseHand(user);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                    throw e;
                }
               
            });
        }

        public void Stop()
        {
            this.context.Release();
            this.close = true;
        }

        public Point3D GetPlayerPosition()
        {
            if (this.positions.ContainsKey(this.playerId))
            {
               // Debugger.Log((int)this.positions[this.playerId].X,"2234","1234");
                return this.positions[this.playerId];
            }
            return new Point3D(0, 0, 0);
        }

        public bool GetFire()
        {
            bool result;

            this.fires.TryGetValue(this.playerId, out result);

            return result;
        }
        //检测是否摆出姿势
        private void UserGenerator_NewUser(object sender, NewUserEventArgs e)
        {
            this.Log("User come.");
            if (this.skeletonCapability.DoesNeedPoseForCalibration)
            {
                this.poseDetectionCapability.StartPoseDetection("Psi", e.ID);
            }
            else
            {
                this.skeletonCapability.RequestCalibration(e.ID, true);
            }

            if (this.OnNewPlayer != null)
            {
                this.OnNewPlayer(this, new PlayerDetectedEventArgs() { PlayerId = e.ID });
            }
        }
        //强制骨骼校准
        private void PoseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e)
        {
            this.poseDetectionCapability.StopPoseDetection(e.ID);
            this.skeletonCapability.RequestCalibration(e.ID, true);
        }
        // 检测到有姿势 强制进行骨骼校准
        private void poseCap_PoseDetected(object sender, PoseDetectedEventArgs e)
        {
            this.poseDetectionCapability.StopPoseDetection(e.ID);
            if (this.playerId == -1)
            {
                this.playerId = e.ID;
                this.skeletonCapability.RequestCalibration(e.ID, true);
            }
        }
        // 跟踪对象  把骨骼位置信息发送给所有观察者
        private void SkeletonCapability_CalibrationComplete(object sender, CalibrationProgressEventArgs e)
        {
            this.Log("Calibration Complete.");
            if (e.Status == CalibrationStatus.OK)
            {
                this.Log("Start tracking");
                this.skeletonCapability.StartTracking(e.ID);
            }
            else if (e.Status == CalibrationStatus.ManualAbort)
            {
                return;
            }
            else
            {
                if (this.skeletonCapability.DoesNeedPoseForCalibration)
                {
                    this.poseDetectionCapability.StartPoseDetection("Psi", e.ID);
                }
                else
                {
                    this.skeletonCapability.RequestCalibration(e.ID, true);
                }
            }
        }
        //// 坐标点
        //private void UpdateUser(int i)
        //{
        //    Point3D point = skeletonCapability.GetSkeletonJointPosition(i, SkeletonJoint.Head).Position;
        //    userDict[i].Head = depthGenerator.ConvertRealWorldToProjective(point);
           
        //    point = skeletonCap.GetSkeletonJointPosition(i, SkeletonJoint.LeftHand).Position;
        //    userDict[i].LeftHand = depthGenerator.ConvertRealWorldToProjective(point);

        //    point = skeletonCap.GetSkeletonJointPosition(i, SkeletonJoint.RightHand).Position;
        //    userDict[i].RightHand = depthGenerator.ConvertRealWorldToProjective(point);

        //    OnUpdateEvent(new UserUpdateEventArgs(userDict[i], UserState.Update));
        //}
        private void UserGenerator_UserExit(object sender, UserExitEventArgs e)
        {
            Point3D point;
            this.positions.TryRemove(e.ID, out point);
            this.Log("User gone.");

            if (this.OnPlayerLost != null)
            {
                this.OnPlayerLost(this, new PlayerDetectedEventArgs() { PlayerId = e.ID });
            }
        }
      
        private void EnqueueRaiseHand(int userId)
        {
            var handPosition = this.depth.ConvertRealWorldToProjective(this.skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.RightHand).Position);
            var headPosition = this.depth.ConvertRealWorldToProjective(this.skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.Head).Position);

            var therhold = 40;

            var handX = handPosition.X;
            var handY = handPosition.Y;

            var headX = headPosition.X;
            var headY = headPosition.Y;

            if (handY.Between(headY + therhold * 2, headY + therhold * 10))
            {
                this.playerId = userId;
                Console.WriteLine(this.playerId);
                // hand raised. detected user.
                if (this.OnPlayerDetected != null)
                {
                    this.OnPlayerDetected(this, new PlayerDetectedEventArgs() { PlayerId = this.playerId });
                }

                this.fires.AddOrUpdate(userId, k => false, (k, v) => false);
            }
            //if (handX.Between(handX -  therhold * 2,handX +  therhold * 2))
            //{
            //    this.playerId = userId;
            //    Console.WriteLine(this.playerId);
            //    if (this.OnPlayerDetected != null)
            //    {
            //        this.OnPlayerDetected(this,new PlayerDetectedEventArgs() {PlayerId = this.playerId });
            //    }
            //    this.fires.AddOrUpdate(userId, k => false,(k,v) => false);
            //}
            else
            {
                bool fire = false;
                if (handX.Between(headX - therhold * 2, headX + therhold * 2)
                     && headY.Between(headY - therhold, headY + therhold))
                {
                    fire = true;
                    this.playerId = userId;
                    this.positions.AddOrUpdate(userId, k => handPosition, (k, v) => handPosition);
                }

                this.fires.AddOrUpdate(userId, k => fire, (k, v) => fire);
            }
        }

        private void EnqueuePos(int userId)
        {
            this.poseDetectionCapability.GetAllAvailablePoses();
        }

        private void Log(string message)
        {
            Debugger.Log(0, "Log", message + "\n");
        }
    }
}
