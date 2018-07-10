using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CommonLib.Numerical
{
    public class Data
    {
        public static void LeastSquaresCoef(double[] Y, double[] X, out double m, out double b, out double stdErrorM, out double stdErrorB)
        {
            int N = X.Length;

            //delta = N*sum(xi^2) - sum(xi)^2
            double delta = X.Length * SumInSquare(X) - Math.Pow(Sum(X), 2.0);

            //b = (1/delta)*(sum(xi^2)*sum(yi) - sum(xi)*sum(xi*yi))
            double bt = (1 / delta) * (SumInSquare(X) * Sum(Y) - Sum(X) * SumInMultiply(Y, X));

            //m = (1/delta)*(N*sum(xi*yi)-sum(xi)*sum(yi))
            double mt = (1 / delta) * (X.Length * SumInMultiply(Y, X) - Sum(X) * Sum(Y));

            m = mt;
            b = bt;

            double var_sum = 0.0;
            for (int i = 0; i < X.Length; i++)
            {
                var_sum += Math.Pow(Y[i] - b - m * X[i], 2);
            }

            double var = 1.0 / (N - 2.0) * var_sum;

            stdErrorB = Math.Sqrt(var / delta * SumInSquare(X));
            stdErrorM = Math.Sqrt(N / delta * var);
        }

        public static double LeastSquaresCoefThroughOrigin(double[] Y, double[] X)
        {
            //m = sum(xi*yi)/sum(xi^2)
            double m = SumInMultiply(Y, X) / SumInSquare(X);
            return m;
        }

        public static double[] LeastSquaresThroughOrigin(double[] Y, double[] X)
        {
            double m = LeastSquaresCoefThroughOrigin(Y, X);

            double[] newY = new double[Y.Length];
            for (int i = 0; i < Y.Length; i++)
                newY[i] = X[i] * m;
            return newY;
        }

        public static double PercentDifference(double x1, double x2)
        {
            var top = x1 - x2;
            var bot = (x1 + x2) / 2.0;
            return Math.Abs(top / bot);
        }

        //
        public static double PercentChange(double newX, double oldX)
        {
            return (newX - oldX) / oldX;
        }

        public static double PercentError(double experimental, double theoretical)
        {
            return (experimental - theoretical) / theoretical;
        }

        public static double PercentErrorAbs(double experimental, double theoretical)
        {
            return Math.Abs(PercentError(experimental, theoretical));
        }

        //http://en.wikipedia.org/wiki/Bessel's_correction
        public static double StandardDeviation(double[] X)
        {
            if (X.Length > 1)
            {
                var mean = X.Average();
                var sum = X.Sum(x => Math.Pow(x - mean, 2));
                var var = sum / (X.Length - 1);

                return Math.Sqrt(var);
            }
            else
                return 0.0;
        }

        /*public static double[] LeastSquaresLine(double[] Y, double[] x) {

		}*/

        private static double Sii(double[] i1, double[] i2)
        {
            return 0;
        }

        private static double Sum(double[] nums)
        {
            double d = 0;
            foreach (double n in nums)
                d += n;

            return d;
        }

        private static double SumInSquare(double[] nums)
        {
            double d = 0;

            for (int i = 0; i < nums.Length; i++)
                d += Math.Pow(nums[i], 2.0);

            return d;
        }

        private static double SumInMultiply(double[] Y, double[] X)
        {
            double d = 0;

            if (Y.Length != X.Length)
                throw new Exception("Lengths must be equal.");

            for (int i = 0; i < X.Length; i++)
                d += (X[i] * Y[i]);

            return d;
        }
    }

    public class PointD
    {
        public PointD() : this(0.0, 0.0) { }

        public PointD(double x, double y) { this.X = x; this.Y = y; }

        public double X;
        public double Y;
        public double Residual;

        public object Tag; //extra data

        public override bool Equals(object obj)
        {
            var p = obj as PointD;
            if (p == null)
                return false;
            return p.X == this.X && p.Y == this.Y;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", X, Y);
        }


    }

    public class XYDataSet : IList<PointD>
	{
		
		private List<PointD> _internalList = new List<PointD>();

		public XYDataSet() : this(null, null) { }

		public XYDataSet(IEnumerable<PointD> points) {
			ResetValues();

			foreach (var point in points)
				Add(point);
		}

		public XYDataSet(IEnumerable<double> Xs, IEnumerable<double> Ys) {
			ResetValues();
			
			if (Xs != null || Ys != null) {
				if (Xs.Count() != Ys.Count())
					throw new Exception("X count must be the same as the Y count.");

				for (int i = 0; i < Xs.Count(); ++i)
					Add(Xs.ElementAt(i), Ys.ElementAt(i));
			}
		}

		public int Count { get { return _internalList.Count; } }

		public bool IsReadOnly { get { return false; } }

		private double _maxX = Double.NegativeInfinity;
		public double XMax { get { return _internalList[XMaxIndex].X; } }
		private double _minX = Double.PositiveInfinity;
		public double XMin { get { return _internalList[XMinIndex].X; } }
		public int XMaxIndex { get; protected set; }
		public int XMinIndex { get; protected set; }

		private double _maxY = Double.NegativeInfinity;
		public double YMax { get { return _internalList[YMaxIndex].Y; } }
		private double _minY = Double.PositiveInfinity;
		public double YMin { get { return _internalList[YMinIndex].Y; } }
		public int YMaxIndex { get; protected set; }
		public int YMinIndex { get; protected set; }

		public double XMean { get { return XSum / Count; } }
		public double YMean { get { return YSum / Count; } }

		public double RSquare { get; protected set; }

		public PointD RegressionPoint0 { get; protected set; }
		public PointD RegressionPointN { get; protected set; }

		public double ResidualStandardDeviation { get; protected set; }

		public double Slope { get; protected set; }

		public double XSum { get; set; }
		public double YSum { get; set; }
		
		public double XSquaredSum { get; set; }
		public double YSquaredSum { get; set; }
		
		public double XYProductSum { get; set; }

		public double XIntercept { get { return -YIntercept / Slope; } }
		public double YIntercept { get; protected set; }


		public PointD this[int index] {
			get { return _internalList[index]; }
			set {
				var p = value;
				var old = _internalList[index];
				_internalList[index] = p;

				ComputeSums(old, SumMode.Subtract);
				ComputeSums(p, SumMode.Add);
				ComputeMinAndMax();
				ComputeSlopeAndYIntercept();
			}
		}

		public void Add(double x, double y) {
			Add(new PointD(x, y));
		}

		public void Add(PointD p) {
			_internalList.Add(p);
			RSquare = double.NaN;

			ComputeSums(p, SumMode.Add);
			ComputeMinAndMax(Count - 1, p);
			ComputeSlopeAndYIntercept();
		}

		public void Clear() {
			_internalList.Clear();
			ResetValues();
		}

		public void ComputeSlopeAndYIntercept() {
			double delta = Count * XSquaredSum - Math.Pow(XSum, 2.0);
			YIntercept = (1.0 / delta) * (XSquaredSum * YSum - XSum * XYProductSum);
			Slope = (1.0 / delta) * (Count * XYProductSum - XSum * YSum);

			RegressionPoint0.X = XMin;
			RegressionPoint0.Y = Slope * XMin + YIntercept;
			RegressionPointN.X = XMax;
			RegressionPointN.Y = Slope * XMax + YIntercept;
		}

		public void ComputeResiduals() {
			for (int i = 0; i < Count; ++i) {
				var yline = Slope * _internalList[i].X + YIntercept;
				var ydelta = _internalList[i].Y - yline;
				_internalList[i].Residual = ydelta;
			}
		}

		public double ComputeRSquared() {
			var SStot = _internalList.Sum(p => Math.Pow(p.Y - YMean, 2.0));
			var SSerr = _internalList.Sum(p => Math.Pow(p.Y - (Slope * p.X + YIntercept), 2.0));
			RSquare = 1.0 - SSerr / SStot;
			return RSquare;
		}

		public double ComputeStandardDeviationOnResiduals(){
			var res = _internalList.Select(p => p.Residual);
			ResidualStandardDeviation = Data.StandardDeviation(res.ToArray());
			return ResidualStandardDeviation;
		}

		public bool Contains(PointD p) {
			return _internalList.Contains(p);
		}

		public void CopyTo(PointD[] points, int index) {
			_internalList.CopyTo(points, index);
		}

		public IEnumerable<PointD> FilterByResidualStandardDeviation(double multiplier) {
			var filter = multiplier * ResidualStandardDeviation;
			var keep = new List<PointD>();
			var reject = new List<PointD>();

			foreach (var p in _internalList)
				if (Math.Abs(p.Residual) < filter)
					keep.Add(p);
				else
					reject.Add(p);

			Clear();
			foreach (var p in keep)
				Add(p);

			return reject;
		}

		public IEnumerator<PointD> GetEnumerator() {
			return _internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _internalList.GetEnumerator();
		}

		public int IndexOf(PointD p) {
			return _internalList.IndexOf(p);
		}

		public void Insert(int index, PointD p) {
			_internalList.Insert(index, p);
			RSquare = double.NaN;

			ComputeSums(p, SumMode.Add);
			ComputeMinAndMax();
			ComputeSlopeAndYIntercept();
		}

		public bool Remove(PointD p) {
			var success = _internalList.Remove(p);
			if (success) {
				RSquare = double.NaN;
				ComputeSums(p, SumMode.Subtract);
				ComputeMinAndMax();
				ComputeSlopeAndYIntercept();
			}
			return success;
		}

		public void RemoveAt(int index) {
			var old = _internalList[index];
			_internalList.RemoveAt(index);
			RSquare = double.NaN;

			ComputeSums(old, SumMode.Subtract);
			ComputeMinAndMax();
			ComputeSlopeAndYIntercept();
		}

		protected void ComputeMinAndMax() { //methods that call this, Insert, 
			ResetMinAndMax();

			for (int i = 0; i < _internalList.Count; ++i)
				ComputeMinAndMax(i, _internalList[i]);
		}

		protected void ComputeMinAndMax(int index, PointD newPoint) {
			if (newPoint.X <= _minX) {
				_minX = newPoint.X;
				XMinIndex = index;
			}

			if (newPoint.X >= _maxX) {
				_maxX = newPoint.X;
				XMaxIndex = index;
			}

			if (newPoint.Y <= _minY) {
				_minY = newPoint.Y;
				YMinIndex = index;
			}

			if (newPoint.Y >= _maxY) {
				_maxY = newPoint.Y;
				YMaxIndex = index;
			}
		}

		protected enum SumMode { Add, Subtract };
		protected void ComputeSums(PointD p, SumMode mode) {
			if (mode == SumMode.Add) {
				XSum += p.X;
				YSum += p.Y;
				XSquaredSum += Math.Pow(p.X, 2.0);
				YSquaredSum += Math.Pow(p.Y, 2.0);
				XYProductSum += (p.X * p.Y);
			}
			else if (mode == SumMode.Subtract) {
				XSum -= p.X;
				YSum -= p.Y;
				XSquaredSum -= Math.Pow(p.X, 2.0);
				YSquaredSum -= Math.Pow(p.Y, 2.0);
				XYProductSum -= (p.X * p.Y);
			}
		}

		protected void ResetMinAndMax() {
			_maxX = double.NegativeInfinity;
			_maxY = double.NegativeInfinity;
			_minX = double.PositiveInfinity;
			_minY = double.PositiveInfinity;
		}

		protected void ResetValues() {
			ResetMinAndMax();

			RegressionPoint0 = new PointD();
			RegressionPointN = new PointD();

			RSquare = double.NaN;

			Slope = double.NaN;
			YIntercept = double.NaN;

			XSum = 0.0;
			YSum = 0.0;
			XSquaredSum = 0.0;
			XYProductSum = 0.0;

			XMaxIndex = -1;
			YMaxIndex = -1;
			XMinIndex = -1;
			YMinIndex = -1;
		}
		
        public void TestXYDataSet()
        {
            double[] X = { 75.0, 83, 85, 85, 92, 97, 99 };
            double[] Y = { 16.0, 20, 25, 27, 32, 48, 48 };
            var ds = new XYDataSet(X, Y);

            Console.WriteLine(Math.Round(ds.Slope, 2)); //1.45
            Console.WriteLine(Math.Round(ds.YIntercept, 2)); //-96.85
            Console.WriteLine(Math.Round(ds.ComputeRSquared(), 3)); //0.927
        } 
	}
}
