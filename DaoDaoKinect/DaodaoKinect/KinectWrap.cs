using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using OpenNI = xn;
using System.IO;

namespace DaodaoKinect {

     public struct Point3D {
          public float X;
          public float Y;
          public float Z;
     }

     public delegate void HandTrack(Point3D location);
     public delegate void GestureRecognize(GestureType gesture, Point3D location);

     public enum GestureType {
          Wave
     }

     public class KinectWrapProperties {
          private bool _hasHandTracking = false;
          public bool HasHandTracking {
               get { return this._hasHandTracking; }
               set { this._hasHandTracking = value; }
          }

          private bool _hasGestureRecognition = false;
          public bool HasGestureRecognition {
               get { return this._hasGestureRecognition; }
               set { this._hasGestureRecognition = value; }
          }

          private HandTrack _handTrackHandler = null;

          public HandTrack HandTrackHandler {
               get { return this._handTrackHandler; }
               set { this._handTrackHandler = value; }
          }

          private GestureRecognize _gestureHandler = null;

          public GestureRecognize GestureHandler {
               get { return this._gestureHandler; }
               set { this._gestureHandler = value; }
          }

          private GestureType[] _gestures = null;

          public GestureType[] Gestures {
               get { return this._gestures; }
               set { this._gestures = value; }
          }

     }

     public class KinectWrap {
          private ResourceManager _resources;
          private string _xmlFileName;

          OpenNI.Context _context;
          OpenNI.HandsGenerator _hands;
          OpenNI.GestureGenerator _gesture;

          /// <summary>
          /// Constructor
          /// </summary>
          public KinectWrap(KinectWrapProperties properties) {
               string xml;
               if (properties == null) {
                    throw new Exception("KinectWrapProperties not provided");
               }
               if (properties.HasGestureRecognition && properties.Gestures == null) {
                    throw new Exception("KinectWrapProperties Gestures must at least have 1 gesture");
               }
               this._resources = new ResourceManager(typeof(XmlFiles));
               xml = this._resources.GetString("openniconfig");
               this._xmlFileName = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "") + ".xml";
               this._xmlFileName = System.Reflection.Assembly.GetExecutingAssembly().Location + this._xmlFileName;
               using (StreamWriter wr = new StreamWriter(this._xmlFileName)) {
                    wr.Write(xml);
               }
               this._context = new OpenNI.Context(this._xmlFileName);
               if (properties.HasHandTracking) {
                    this._hands = new OpenNI.HandsGenerator(this._context);
                    this._hands.HandUpdate += new OpenNI.HandsGenerator.HandUpdateHandler(
                         delegate(OpenNI.ProductionNode node, uint id, ref OpenNI.Point3D position, float fTime) {
                              if (properties.HandTrackHandler != null) {
                                   properties.HandTrackHandler.Invoke(new Point3D() { X = position.X, Y = position.Y, Z = position.Z });
                              }
                         });
                    if (!properties.HasGestureRecognition) {
                         this._hands.HandCreate += new OpenNI.HandsGenerator.HandCreateHandler(
                              delegate(OpenNI.ProductionNode node, uint id, ref OpenNI.Point3D position, float fTime) {
                                   this._hands.StartTracking(ref position);                                   
                              });
                    }
               }
               if (properties.HasGestureRecognition) {
                    this._gesture = new OpenNI.GestureGenerator(this._context);
                    foreach (GestureType g in properties.Gestures) {
                         switch (g) {
                              case GestureType.Wave: {
                                        this._gesture.AddGesture("Wave");
                                        break;
                                   }
                              default: {
                                        break;
                                   }
                         }
                    }
                    this._gesture.GestureRecognized += new OpenNI.GestureGenerator.GestureRecognizedHandler(
                         delegate(OpenNI.ProductionNode node, string strGesture, ref OpenNI.Point3D idPosition, ref OpenNI.Point3D endPosition) {
                              if (properties.GestureHandler != null) {
                                   GestureType gt;
                                   switch (strGesture) {
                                        case "Wave": {
                                                  gt = GestureType.Wave;
                                                  break;
                                             }
                                        default: {
                                                  gt = GestureType.Wave;
                                                  break;
                                             }
                                   }
                                   properties.GestureHandler.Invoke(gt, new Point3D() { X = endPosition.X, Y = endPosition.Y, Z = endPosition.Z });
                              }
                              if (properties.HasHandTracking) {
                                   this._hands.StartTracking(ref endPosition);
                              }
                         });
               }
               if (properties.HasHandTracking) {
                    this._hands.StartGenerating();
               }
               if (properties.HasGestureRecognition) {
                    this._gesture.StartGenerating();
               }
               System.Threading.Thread th = new System.Threading.Thread(delegate() {
                    try {
                         while (true) {
                              this._context.WaitAnyUpdateAll();
                         }
                    }
                    finally {
                         this.Dispose();
                    }
               });
               th.Start();
          }

          /// <summary>
          /// Destructor
          /// </summary>
          public void Dispose() {
               File.Delete(this._xmlFileName);
               if (this._hands != null) {
                    this._hands.Dispose();
                    this._hands = null;
               }
               if (this._gesture != null) {
                    this._gesture.Dispose();
                    this._gesture = null;
               }
               this._context.Dispose();
               this._context = null;
          }
     }
}
