using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine
{
    public static class Helper
    {
		internal static bool MicrosoftCLR { get; } = Type.GetType("Mono.Runtime") == null;

        public static float Distance(this Vector2 thisPosition, Vector2 point)
        {
            var dX = thisPosition.X - point.X;
            var dY = thisPosition.Y - point.Y;
            return (float)Math.Sqrt((dX * dX) + (dY * dY));
        }

        public static float Dot(this Vector2 thisPosition, Vector2 point)
        {
            var dX = thisPosition.X * point.X;
            var dY = thisPosition.Y * point.Y;
            return dX + dY;
        }

        public static T SelectMin<T, T2>(this IEnumerable<T> collection, Func<T, T2> select) where T2 : IComparable
        {
            if (collection.Count() == 0) return default(T);

            T currentBest = collection.First();
            T2 bestValue = select(currentBest);

            foreach (var current in collection.Skip(1))
            {
                T2 currentValue = select(current);
                if (currentValue.CompareTo(bestValue) <= 0)
                {
                    bestValue = currentValue;
                    currentBest = current;
                }
            }

            return currentBest;
        }

        public static T SelectMax<T, T2>(this IEnumerable<T> collection, Func<T, T2> select) where T2 : IComparable
        {
            if (collection.Count() == 0) return default(T);

            T currentBest = collection.First();
            T2 bestValue = select(currentBest);

            foreach (var current in collection.Skip(1))
            {
                T2 currentValue = select(current);
                if (currentValue.CompareTo(bestValue) >= 0)
                {
                    bestValue = currentValue;
                    currentBest = current;
                }
            }

            return currentBest;
        }

        public static void Swap<T>(ref T item1, ref T item2)
        {
            T aux = item1;
            item1 = item2;
            item2 = aux;
        }   

        /// <summary>
        /// Determines if the given point is inside the polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsPointInPolygon4(Vector2[] polygon, Vector2 testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public static Vector2[] PolygonFromLine(Vector2 point1, Vector2 point2, float width)
        {
            //https://www.codeproject.com/Articles/199525/Drawing-nearly-perfect-D-line-segments-in-OpenGL

            double w = width, x1 = point1.X, x2 = point2.X, y1 = point1.Y, y2 = point2.Y;
            double t = 0; double f = w - (int)w;

            //determine parameters t
            if (w >= 1.0 && w < 2.0)
            {
                t = 0.05 + f * 0.33;
            }
            else if (w >= 2.0 && w < 3.0)
            {
                t = 0.38 + f * 0.58; 
            }
            else if (w >= 3.0 && w < 4.0)
            {
                t = 0.96 + f * 0.48; 
            }
            else if (w >= 4.0 && w < 5.0)
            {
                t = 1.44 + f * 0.46; 
            }
            else if (w >= 5.0 && w < 6.0)
            {
                t = 1.9 + f * 0.6; 
            }
            else if (w >= 6.0)
            {
                double ff = w - 6.0;
                t = 2.5 + ff * 0.50;
            }
            //printf( "w=%f, f=%f, C=%.4f\n", w,f,C);

            //determine angle of the line to horizontal
            double tx = 0, ty = 0; //core thinkness of a line
            double cx = 0, cy = 0; //cap of a line
            double ALW = 0.01;
            double dx = x2 - x1;
            double dy = y2 - y1;
            if (Math.Abs(dx) < ALW)
            {
                //vertical
                tx = t; ty = 0;
                if (w > 0.0 && w <= 1.0)
                {
                    tx = 0.5; 
                }
            }
            else if (Math.Abs(dy) < ALW)
            {
                //horizontal
                tx = 0; ty = t;
                if (w > 0.0 && w <= 1.0)
                {
                    ty = 0.5;
                }
            }
            else
            {
                if (w < 3)
                { //approximate to make things even faster
                    double m = dy / dx;
                    //and calculate tx,ty,Rx,Ry
                    if (m > -0.4142 && m <= 0.4142)
                    {
                        // -22.5< angle <= 22.5, approximate to 0 (degree)
                        tx = t * 0.1; ty = t;
                    }
                    else if (m > 0.4142 && m <= 2.4142)
                    {
                        // 22.5< angle <= 67.5, approximate to 45 (degree)
                        tx = t * -0.7071; ty = t * 0.7071;
                    }
                    else if (m > 2.4142 || m <= -2.4142)
                    {
                        // 67.5 < angle <=112.5, approximate to 90 (degree)
                        tx = t; ty = t * 0.1;
                    }
                    else if (m > -2.4142 && m < -0.4142)
                    {
                        // 112.5 < angle < 157.5, approximate to 135 (degree)
                        tx = t * 0.7071; ty = t * 0.7071;
                    }
                }
                else
                { //calculate to exact
                    dx = y1 - y2;
                    dy = x2 - x1;
                    double L = Math.Sqrt(dx * dx + dy * dy);
                    dx /= L;
                    dy /= L;
                    cx = -dy; cy = dx;
                    tx = t * dx; ty = t * dy;
                }
            }

            x1 += cx * 0.5; y1 += cy * 0.5;
            x2 -= cx * 0.5; y2 -= cy * 0.5;


            return new Vector2[]
             {
                new Vector2((float)(x1-tx-cx),(float)(y1-ty-cy)),
                new Vector2((float)(x2-tx+cx),(float)(y2-ty+cy)),
                new Vector2((float)(x2+tx+cx),(float)(y2+ty+cy)),
                new Vector2((float)(x1+tx-cx),(float)(y1+ty-cy))
             };
        }
    }
}
