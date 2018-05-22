using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.OCR;

namespace Project2
{
    public class Shape
    {
        public string ShapeD(string filename)
        {
            StringBuilder msgBuilder = new StringBuilder("Performance: ");

            Image<Bgr, Byte> img = new Image<Bgr, byte>(filename);
            img = img.Resize(img.Width * 2, img.Height * 2, Emgu.CV.CvEnum.Inter.Linear, true);
           
            Stopwatch watch = Stopwatch.StartNew();
            double cannyThreshold = 180.0;
            
            #region Canny and edge detection
            watch.Reset(); watch.Start();
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(img, cannyEdges, cannyThreshold, cannyThresholdLinking);
            cannyEdges.Save("cannyEdges.png");

            LineSegment2D[] lines = CvInvoke.HoughLinesP(cannyEdges, 1, Math.PI/45.0, 20, 0, 0);

            watch.Stop();
            msgBuilder.Append(String.Format("Canny & Hough lines - {0} ms; ", watch.ElapsedMilliseconds));
            #endregion

            #region Find triangles and rectangles
            watch.Reset(); watch.Start();
            List<RotatedRect> boxList = new List<RotatedRect>(); 

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (CvInvoke.ContourArea(approxContour, false) > 250) 
                        {
                            if (approxContour.Size == 4) 
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion

                                if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                            }
                        }
                    }
                }
            }

            watch.Stop();
            msgBuilder.Append(String.Format("Rectangles - {0} ms; ", watch.ElapsedMilliseconds));
            #endregion

            var originalImageBox = new ImageBox { Image = img };
            var result = msgBuilder.ToString();
            List<IInputOutputArray> licensePlateImagesList = new List<IInputOutputArray>();

            #region draw rectangles
            Image<Bgr, Byte> triangleRectangleImage = img.CopyBlank();
            Image<Bgr, Byte> rect = img.Copy();
            foreach (RotatedRect box in boxList)
            {

                rect.Data[(int)box.Center.Y - (int)box.Size.Height/2, (int)box.Center.X - (int)box.Size.Width / 2, 0]
                    = img.Data[(int)box.Center.Y - (int)box.Size.Height / 2, (int)box.Center.X - (int)box.Size.Width / 2, 0];

                rect.Save("002.png");

                licensePlateImagesList.Add(triangleRectangleImage);
                triangleRectangleImage.Draw(box, new Bgr(Color.DarkOrange), 2);
            }

            var lic = new LicPlateRecog.LicensePlateDetector();
            var licoutput = lic.DetectLicensePlate(img, licensePlateImagesList, licensePlateImagesList, boxList);

            triangleRectangleImage.Save("rectangles.png");
            #endregion

            #region draw lines
            Image<Bgr, Byte> lineImage = img.CopyBlank();
            foreach (LineSegment2D line in lines)
                lineImage.Draw(line, new Bgr(Color.Green), 2);
            lineImage.Save("lines.png");
            #endregion

            return result;
        }

    }
}
