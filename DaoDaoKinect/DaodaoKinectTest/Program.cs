using System;
using System.Collections.Generic;
using System.Text;
using dk = DaodaoKinect;

namespace DaodaoKinectTest {

     // Main program
     class Program {

          static void Main(string[] args) {
               dk.KinectWrapProperties kwp;
               dk.KinectWrap kw;

               kwp = new dk.KinectWrapProperties();
               kwp.HasHandTracking = true;
               kwp.HasGestureRecognition = true;
               kwp.HandTrackHandler = HandTrack;
               kwp.GestureHandler = GestureHandle;
               kwp.Gestures = new dk.GestureType[1] { dk.GestureType.Wave };
               kw = new dk.KinectWrap(kwp);
          }

          static void HandTrack(dk.Point3D position) {
               Console.WriteLine(@"Hand position - X: {0}, Y:{1}, Z:{2}", position.X, position.Y, position.Z);
          }

          static void GestureHandle(dk.GestureType gesture, dk.Point3D position) {
               Console.WriteLine(@"Gesture recognized...");
          }

     }

}
