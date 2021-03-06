﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace FaceAPI
{
    class FaceIntegration
    {
        private Image<Bgr, byte> faceImgA, faceImgB, dstFace;
        private Image<Gray, byte> dstMask;
        private double integrationRatio;
        private Size srcSize, dstSize;
        private PointF[] landmarkA, landmarkB;
        private PointF[] srcLandmarkA, srcLandmarkB, dstLandmark;
        private PointF[,] triangularSetA, triangularSetB;
        private PointF[,] quadrangularSetA, quadrangularSetB;

        private const int POINTNUM = 27;
        private const int TRIANGULARNUM = 10;
        private const int QUADRANGULARNUM = 3;

        public FaceIntegration(
            Image<Bgr, byte> _faceImgA,
            Image<Bgr, byte> _faceImgB,
            PointF[] _landmarkA,
            PointF[] _landmarkB,
            Size _size,
            double _integrationRatio)
        {
            faceImgA = _faceImgA.Clone();
            faceImgB = _faceImgB.Clone();
            landmarkA = _landmarkA;
            landmarkB = _landmarkB;
            srcSize = _size;
            integrationRatio = _integrationRatio;

            setSrcFaceParam();
            setDstFaceParam();

            setSrcTriangularSet();
            setSrcQuadrangularSet();
        }

        public Image<Bgr, byte> integrateFace()
        {
            for (int count = 0; count < TRIANGULARNUM; count++)
            {
                PointF[] triPointA = new PointF[3];
                PointF[] triPointB = new PointF[3];
                for (int countInner = 0; countInner < 3; countInner++)
                {
                    triPointA[countInner] = triangularSetA[count, countInner];
                    triPointB[countInner] = triangularSetB[count, countInner];
                }
                integrateRegion(triPointA, triPointB, 3);
            }

            for (int count = 0; count < QUADRANGULARNUM; count++)
            {
                PointF[] quadPointA = new PointF[4];
                PointF[] quadPointB = new PointF[4];
                for (int countInner = 0; countInner < 4; countInner++)
                {
                    quadPointA[countInner] = quadrangularSetA[count, countInner];
                    quadPointB[countInner] = quadrangularSetB[count, countInner];
                }
                integrateRegion(quadPointA, quadPointB, 4);
            }

            return dstFace;
        }

        private void setSrcFaceParam()
        {
            faceImgA = faceImgA.Resize(srcSize.Width, srcSize.Height, Inter.Linear);
            faceImgB = faceImgB.Resize(srcSize.Width, srcSize.Height, Inter.Linear);

            srcLandmarkA = new PointF[POINTNUM];
            for (int cnt = 0; cnt < POINTNUM; cnt++)
            {
                srcLandmarkA[cnt] = new PointF(
                    (float)(landmarkA[cnt].X * srcSize.Width),
                    (float)(landmarkA[cnt].Y * srcSize.Height));
            }

            srcLandmarkB = new PointF[POINTNUM];
            for (int cnt = 0; cnt < POINTNUM; cnt++)
            {
                srcLandmarkB[cnt] = new PointF(
                    (float)(landmarkB[cnt].X * srcSize.Width),
                    (float)(landmarkB[cnt].Y * srcSize.Height));
            }
        }

        private void setDstFaceParam()
        {
            dstSize = srcSize;
            dstFace = new Image<Bgr, byte>(dstSize);
            dstFace.SetZero();
            dstMask = new Image<Gray, byte>(dstSize);
            dstMask.SetZero();

            dstLandmark = new PointF[POINTNUM];
            for (int cnt = 0; cnt < POINTNUM; cnt++)
            {
                dstLandmark[cnt] = new PointF(
                    (float)((landmarkA[cnt].X + landmarkB[cnt].X) * srcSize.Width * integrationRatio),
                    (float)((landmarkA[cnt].Y + landmarkB[cnt].Y) * srcSize.Height * (1 - integrationRatio)));
            }
        }

        private void setSrcTriangularSet()
        {
            triangularSetA = new PointF[TRIANGULARNUM, 3]{
                {landmarkA[(int)Position.EyebrowLeftOuter],
                landmarkA[(int)Position.PupilLeft],
                landmarkA[(int)Position.MouthLeft]},
                {landmarkA[(int)Position.EyebrowRightOuter],
                landmarkA[(int)Position.PupilRight],
                landmarkA[(int)Position.MouthRight]},
                {landmarkA[(int)Position.MouthLeft],
                landmarkA[(int)Position.MouthRight],
                landmarkA[(int)Position.UnderLipBottom]},
                {new PointF(0, 1),
                landmarkA[(int)Position.MouthLeft],
                landmarkA[(int)Position.UnderLipBottom]},
                {new Point(1, 1),
                landmarkA[(int)Position.MouthRight],
                landmarkA[(int)Position.UnderLipBottom]},
                {landmarkA[(int)Position.UnderLipBottom],
                new PointF(0, 1),
                new PointF(1, 1)},
                {landmarkA[(int)Position.EyebrowLeftOuter],
                landmarkA[(int)Position.MouthLeft],
                new PointF(0, 1)},
                {new PointF(0, 0),
                 new PointF(0, 1),
                 landmarkA[(int)Position.EyebrowLeftOuter]},
                {landmarkA[(int)Position.EyebrowRightOuter],
                landmarkA[(int)Position.MouthRight],
                new PointF(1, 1)},
                {new PointF(1, 1),
                 new PointF(1, 0),
                 landmarkA[(int)Position.EyebrowRightOuter]}
            };

            triangularSetB = new PointF[TRIANGULARNUM, 3]{
                {landmarkB[(int)Position.EyebrowLeftOuter],
                landmarkB[(int)Position.PupilLeft],
                landmarkB[(int)Position.MouthLeft]},
                {landmarkB[(int)Position.EyebrowRightOuter],
                landmarkB[(int)Position.PupilRight],
                landmarkB[(int)Position.MouthRight]},
                {landmarkB[(int)Position.MouthLeft],
                landmarkB[(int)Position.MouthRight],
                landmarkB[(int)Position.UnderLipBottom]},
                {new PointF(0, 1),
                landmarkB[(int)Position.MouthLeft],
                landmarkB[(int)Position.UnderLipBottom]},
                {new Point(1, 1),
                landmarkB[(int)Position.MouthRight],
                landmarkB[(int)Position.UnderLipBottom]},
                {landmarkB[(int)Position.UnderLipBottom],
                new PointF(0, 1),
                new PointF(1, 1)},
                {landmarkB[(int)Position.EyebrowLeftOuter],
                landmarkB[(int)Position.MouthLeft],
                new PointF(0, 1)},
                {new PointF(0, 0),
                 new PointF(0, 1),
                 landmarkB[(int)Position.EyebrowLeftOuter]},
                {landmarkB[(int)Position.EyebrowRightOuter],
                landmarkB[(int)Position.MouthRight],
                new PointF(1, 1)},
                {new PointF(1, 1),
                 new PointF(1, 0),
                 landmarkB[(int)Position.EyebrowRightOuter]}
            };
        }

        private void setSrcQuadrangularSet()
        {
            quadrangularSetA = new PointF[QUADRANGULARNUM, 4] {
                {landmarkA[(int)Position.EyebrowLeftOuter],
                landmarkA[(int)Position.EyebrowRightOuter],
                landmarkA[(int)Position.PupilRight],
                landmarkA[(int)Position.PupilLeft]},
                {landmarkA[(int)Position.PupilLeft],
                landmarkA[(int)Position.PupilRight],
                landmarkA[(int)Position.MouthRight],
                landmarkA[(int)Position.MouthLeft]},
                {landmarkA[(int)Position.EyebrowLeftOuter],
                landmarkA[(int)Position.EyebrowRightOuter],
                new PointF(1, 0),
                new PointF(0, 0)}
            };

            quadrangularSetB = new PointF[QUADRANGULARNUM, 4] {
                {landmarkB[(int)Position.EyebrowLeftOuter],
                landmarkB[(int)Position.EyebrowRightOuter],
                landmarkB[(int)Position.PupilRight],
                landmarkB[(int)Position.PupilLeft]},
                {landmarkB[(int)Position.PupilLeft],
                landmarkB[(int)Position.PupilRight],
                landmarkB[(int)Position.MouthRight],
                landmarkB[(int)Position.MouthLeft]},
                {landmarkB[(int)Position.EyebrowLeftOuter],
                landmarkB[(int)Position.EyebrowRightOuter],
                new PointF(1, 0),
                new PointF(0, 0)}
            };
        }

        private void integrateRegion(PointF[] _pointA, PointF[] _pointB, int _polyCount)
        {
            PointF[] pointA = convertPointF(_pointA, _polyCount);
            Image<Gray, byte> maskA = new Image<Gray, byte>(srcSize);
            VectorOfVectorOfPoint pointSetA = new VectorOfVectorOfPoint(new VectorOfPoint(convertPointF2Point(pointA, _polyCount)));
            CvInvoke.FillPoly(maskA, pointSetA, new MCvScalar(255), LineType.EightConnected);
            Image<Bgr, byte> tempA = new Image<Bgr, byte>(srcSize);
            tempA = faceImgA.Copy(maskA);

            PointF[] pointB = convertPointF(_pointB, _polyCount);
            Image<Gray, byte> maskB = new Image<Gray, byte>(srcSize);
            VectorOfVectorOfPoint pointSetB = new VectorOfVectorOfPoint(new VectorOfPoint(convertPointF2Point(pointB, _polyCount)));
            CvInvoke.FillPoly(maskB, pointSetB, new MCvScalar(255), LineType.EightConnected);
            Image<Bgr, byte> tempB = new Image<Bgr, byte>(srcSize);
            tempB = faceImgB.Copy(maskB);

            PointF[] dstPoint = getDstPoint(pointA, pointB, _polyCount);

            Mat srcRotMatA = new Mat();
            Mat srcRotMatB = new Mat();

            if (_polyCount == 3)
            {
                srcRotMatA = CvInvoke.GetAffineTransform(pointA, dstPoint);
                srcRotMatB = CvInvoke.GetAffineTransform(pointB, dstPoint);
            }
            else if (_polyCount == 4)
            {
                srcRotMatA = CvInvoke.GetPerspectiveTransform(pointA, dstPoint);
                srcRotMatB = CvInvoke.GetPerspectiveTransform(pointB, dstPoint);
            }
            Image<Bgr, byte> srcWarpA = new Image<Bgr, byte>(dstSize);
            Image<Bgr, byte> srcWarpB = new Image<Bgr, byte>(dstSize);
            Image<Gray, byte> maskWarpA = new Image<Gray, byte>(dstSize);
            Image<Gray, byte> maskWarpB = new Image<Gray, byte>(dstSize);
            srcWarpA.SetZero();
            srcWarpB.SetZero();
            maskWarpA.SetZero();
            maskWarpB.SetZero();

            if (_polyCount == 3)
            {
                CvInvoke.WarpAffine(tempA, srcWarpA, srcRotMatA, dstSize);
                CvInvoke.WarpAffine(tempB, srcWarpB, srcRotMatB, dstSize);
                CvInvoke.WarpAffine(maskA, maskWarpA, srcRotMatA, dstSize);
                CvInvoke.WarpAffine(maskB, maskWarpB, srcRotMatB, dstSize);
            }
            else if (_polyCount == 4)
            {
                CvInvoke.WarpPerspective(tempA, srcWarpA, srcRotMatA, dstSize);
                CvInvoke.WarpPerspective(tempB, srcWarpB, srcRotMatB, dstSize);
                CvInvoke.WarpPerspective(maskA, maskWarpA, srcRotMatA, dstSize);
                CvInvoke.WarpPerspective(maskB, maskWarpB, srcRotMatB, dstSize);
            }

            maskWarpA = maskWarpA - (maskWarpA & dstMask) * 255;
            maskWarpB = maskWarpB - (maskWarpB & dstMask) * 255;
            dstMask = dstMask + (maskWarpA & maskWarpB) * 255;
            srcWarpA = srcWarpA.Copy(maskWarpA);
            srcWarpB = srcWarpB.Copy(maskWarpB);
            CvInvoke.AddWeighted(srcWarpA, integrationRatio, srcWarpB, 1 - integrationRatio, 0, srcWarpA);
            CvInvoke.Add(dstFace, srcWarpA, dstFace);
        }

        private PointF[] convertPointF(PointF[] _pointSet, int _size)
        {
            PointF[] retPointSet = new PointF[_size];

            for (int count = 0; count < _size; count++)
            {
                retPointSet[count].X = _pointSet[count].X * (float)srcSize.Width;
                retPointSet[count].Y = _pointSet[count].Y * (float)srcSize.Height;
            }

            return retPointSet;
        }

        private PointF[] getDstPoint(PointF[] _pointA, PointF[] _pointB, int _size)
        {
            PointF[] retPoint = new PointF[_size];

            for (int count = 0; count < _size; count++)
            {
                retPoint[count].X = ((float)(_pointA[count].X * integrationRatio) + (float)(_pointB[count].X * (1 - integrationRatio)));
                retPoint[count].Y = ((float)(_pointA[count].Y * integrationRatio) + (float)(_pointB[count].Y * (1 - integrationRatio)));
            }

            return retPoint;
        }

        private Point[] convertPointF2Point(PointF[] _pointF, int _size)
        {
            Point[] retPoint = new Point[_size];

            for (int count = 0; count < _size; count++)
            {
                retPoint[count].X = (int)_pointF[count].X;
                retPoint[count].Y = (int)_pointF[count].Y;
            }

            return retPoint;
        }

    }
}
