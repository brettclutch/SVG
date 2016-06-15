using System;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidMatrix : Matrix
    {
        private Android.Graphics.Matrix _m;
        public AndroidMatrix()
        {
            _m = new Android.Graphics.Matrix();
        }
        public AndroidMatrix(Android.Graphics.Matrix src)
        {
            //_m = new Android.Graphics.Matrix(src);
            _m = src;
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Drawing.Drawing2D.Matrix class with
        //     the specified elements.
        //
        // Parameters:
        //   m11:
        //     The value in the first row and first column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   m12:
        //     The value in the first row and second column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   m21:
        //     The value in the second row and first column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   m22:
        //     The value in the second row and second column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   dx:
        //     The value in the third row and first column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   dy:
        //     The value in the third row and second column of the new System.Drawing.Drawing2D.Matrix.
        public AndroidMatrix(float i, float i1, float i2, float i3, float i4, float i5)
        {
            _m = new Android.Graphics.Matrix();
            _m.SetValues(new float[] { i, i1, i3, i4, i2, i5, 0, 0, 1 });
        }

        public override void Dispose()
        {
            _m.Dispose();
        }

        public override void Scale(float width, float height)
        {
            _m.SetScale(width, height);
        }

        public override void Scale(float width, float height, MatrixOrder order)
        {
            if(order == MatrixOrder.Append)
                _m.PostScale(width, height);
            else
                _m.PreScale(width, height);
        }

        public override void Translate(float left, float top, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
                _m.PostTranslate(left, top);
            else
                _m.PreTranslate(left, top);
        }

        public override void TransformVectors(PointF[] points)
        {
            var a = new float[points.Length*2];
            for (int i = 0; i < points.Length; i++)
            {
                a[i*2] = points[i].X;
                a[i * 2 + 1] = points[i].Y;
            }

            _m.MapVectors(a);

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = a[i * 2];
                points[i].Y = a[i * 2 + 1];
            }
        }

        public override void Translate(float left, float top)
        {
            _m.SetTranslate(left, top);
        }

        public override void Multiply(Matrix matrix)
        {
            var other = (AndroidMatrix) matrix;

            var a = new float[9];
            var b = new float[9];
            _m.GetValues(a);
            other._m.GetValues(b);

            var a1 = new float[3, 3];
            a1[0, 0] = a[0];
            a1[0, 1] = a[1];
            a1[0, 2] = a[2];
            a1[1, 0] = a[3];
            a1[1, 1] = a[4];
            a1[1, 2] = a[5];
            a1[2, 0] = a[6];
            a1[2, 1] = a[7];
            a1[2, 2] = a[8];
            
            var b1 = new float[3, 3];
            b1[0, 0] = b[0];
            b1[0, 1] = b[1];
            b1[0, 2] = b[2];
            b1[1, 0] = b[3];
            b1[1, 1] = b[4];
            b1[1, 2] = b[5];
            b1[2, 0] = b[6];
            b1[2, 1] = b[7];
            b1[2, 2] = b[8];


            var r = MultiplyMatrix(a1, b1);
            _m.SetValues(new float[]
            {
                r[0,0], r[0,1], r[0,2], 
                r[1,0], r[1,1], r[1,2], 
                r[2,0], r[2,1], r[2,2], 
                
            });
        }

        public float[,] MultiplyMatrix(float[,] a, float[,] b)
        {
            int rA = a.GetLength(0);
            int cA = a.GetLength(1);
            int rB = b.GetLength(0);
            int cB = b.GetLength(1);
            float temp = 0;
            float[,] kHasil = new float[rA, cB];
            if (cA != rB)
            {
                throw new InvalidOperationException("matrix cannot be multiplied");
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += a[i, k] * b[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }
                return kHasil;
            }
        }
        
        public override void TransformPoints(PointF[] points)
        {
            TransformVectors(points);
        }

        public override void RotateAt(float angle, PointF midPoint, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
                _m.PostRotate(angle, midPoint.X, midPoint.Y);
            else
                _m.PreRotate(angle, midPoint.X, midPoint.Y);
        }

        public override void Rotate(float angle, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
                _m.PostRotate(angle);
            else
                _m.PreRotate(angle);
        }

        public override void Rotate(float fAngle)
        {
            _m.SetRotate(fAngle);
        }

        public override Matrix Clone()
        {
            return new AndroidMatrix(_m);
        }

        public override float[] Elements
        {
            get
            {
                var res = new float[9];
                _m.GetValues(res);
                return res;
            }
        }

        public override float OffsetX
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MtransX];
            }
        }

        public override float OffsetY
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MtransY];
            }
        }

        public override float ScaleX
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MscaleX];
            }
        }

        public override float ScaleY
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MscaleY];
            }
        }

        public Android.Graphics.Matrix Matrix { get { return _m; }}

        public override void Shear(float f, float f1)
        {
            _m.SetSkew(f, f1);
        }

    }
}