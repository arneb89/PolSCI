using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Maper.StokesImaging
{
    class StokesMapper
    {
        private MagnetizedSurface magSrf = null;
        private Stokes_T_F_B_Theta_Lambda stokes_func = null;
        private StokesCurve[] curves;
        private StokesCurve[] curvesRes;
        private MagnetizedSurface magSrfRes = null;
        private double scale_koeff;
        private double sizePar = 0;

        public StokesMapper(MagnetizedSurface magSrf, 
            Stokes_T_F_B_Theta_Lambda stokes_func, 
            StokesCurve[] curves)
        {
            this.magSrf = magSrf;
            this.stokes_func = stokes_func;
            this.curves = curves;
            this.curvesRes = new StokesCurve[curves.Length];

            for (int q = 0; q < this.curvesRes.Length; q++)
            {
                this.curvesRes[q] = new StokesCurve();
                this.curvesRes[q].filter = curves[q].filter;
                this.curvesRes[q].phases = curves[q].phases;
                this.curvesRes[q].type = curves[q].type;
            }

            this.magSrfRes = new MagnetizedSurface(
                magSrf.InclinationOfRotationAxis,
                magSrf.LatitudeDipolOffset,
                magSrf.LongitudeDipolOffset,
                magSrf.PolesMagneticField,
                magSrf.NLat,
                magSrf.NLong, 1);
        }

        public void StartMapping(double regPar, double lambdaPole, string pathNNLSDir)
        {
            this.sizePar = lambdaPole;
            double aveFlux = curves[0].value.Average();
            int allPhases = 0;
            for (int p = 0; p < this.curves.Length; p++)
            {
                allPhases += this.curves[p].phases.Length;
            }

            int observablePatchesNumber = this.magSrf.GetNumberOfObservablePatches();
            int observableLatBeltsNumber = this.magSrf.GetNumberOfObservableLatBelts();

            double[][] a = new double[allPhases][];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = new double[observablePatchesNumber];
            }

            {
                int s = 0;
                for (int q = 0; q < this.curves.Length; q++)
                {
                    for (int p = 0; p < this.curves[q].phases.Length; p++)
                    {
                        int k = 0;

                        double muPole = this.magSrf.MuOfThePole(this.curves[q].phases[p]);
                        double sinGammaCenter = Math.Sqrt(1 - muPole * muPole);
                        if (sinGammaCenter > 1.0) sinGammaCenter = 1.0;
                        if (sinGammaCenter < -1.0) sinGammaCenter = -1.0;

                        double cosKhi = this.magSrf.GetCosOmega(this.curves[q].phases[p], muPole);
                        double sinKhi = 0;
                        double eps = this.magSrf.PhiOfThePole(this.curves[q].phases[p])
                            - 2 * Math.PI * Math.Floor(this.magSrf.PhiOfThePole(this.curves[q].phases[p]) / (2 * Math.PI));

                        if (eps < Math.PI / 2.0 || eps > 1.5 * Math.PI)
                        {
                            sinKhi = -Math.Sqrt(1 - cosKhi * cosKhi);
                        }
                        else
                        {
                            sinKhi = Math.Sqrt(1 - cosKhi * cosKhi);
                        }

                        for (int i = 0; i < observableLatBeltsNumber; i++)
                        {
                            for (int j = 0; j < this.magSrf.Patches[i].Length; j++)
                            {
                                double mu = magSrf.MuOfThePatchCenter(i, j, muPole, sinGammaCenter, cosKhi, sinKhi);

                                if (mu > 0)
                                {
                                    double area = (this.magSrf.Patches[i][j].Phi20 - this.magSrf.Patches[i][j].Phi10) *
                                (Math.Cos(this.magSrf.Patches[i][j].Theta1) - Math.Cos(this.magSrf.Patches[i][j].Theta2));
                                    double cosAlpha = this.magSrf.GetCosAlpha2(i, j, this.curves[q].phases[p]);
                                    double alpha = Math.Acos(cosAlpha);
                                    double prArea = area*mu;

                                    double koeff_i, koeff_q, koeff_v, koeff_u;
                                    if (this.curves[q].type == "I")
                                    {
                                        if (alpha > 0.5 * Math.PI)
                                        {
                                            koeff_i = this.stokes_func("I", this.curves[q].filter,
                                                this.magSrf.MagneticStrength(i, j) * 1e-6,
                                            Math.PI - alpha,
                                            this.magSrf.Lambda(i, j, lambdaPole));
                                        }
                                        else
                                        {
                                            koeff_i = this.stokes_func("I", this.curves[q].filter,
                                                this.magSrf.MagneticStrength(i, j) * 1e-6,
                                            alpha,
                                            this.magSrf.Lambda(i, j, lambdaPole));
                                        }
                                        a[s][k] = prArea * koeff_i;
                                    }
                                    if (this.curves[q].type == "V")
                                    {
                                        if (alpha <= Math.PI * 0.5)
                                        {
                                            a[s][k] = prArea * this.stokes_func("V", this.curves[q].filter,
                                                this.magSrf.MagneticStrength(i, j) * 1e-6,
                                            alpha,
                                            this.magSrf.Lambda(i, j, lambdaPole));
                                        }
                                        else
                                        {
                                            a[s][k] = -prArea * this.stokes_func("V", this.curves[q].filter,
                                                this.magSrf.MagneticStrength(i, j) * 1e-6,
                                            Math.PI - alpha,
                                            this.magSrf.Lambda(i, j, lambdaPole));
                                        }
                                    }
                                    if (this.curves[q].type == "Q")
                                    {
                                        double ro = this.magSrf.GetRo(i, j, this.curves[q].phases[p]);
                                        double alpha1 = alpha;
                                        if (alpha > 0.5 * Math.PI) alpha1 = Math.PI - alpha;
                                        a[s][k] = area * Math.Cos(2 * ro) * this.stokes_func("Q", this.curves[q].filter,
                                            this.magSrf.MagneticStrength(i, j),
                                            alpha1,
                                            this.magSrf.Lambda(i, j, lambdaPole));
                                    }
                                    if (this.curves[q].type == "U")
                                    {
                                        double ro = this.magSrf.GetRo(i, j, this.curves[q].phases[p]);
                                        double alpha1 = alpha;
                                        if (alpha > 0.5 * Math.PI) alpha1 = Math.PI - alpha;
                                        a[s][k] = -area * Math.Sin(2 * ro) * this.stokes_func("Q", this.curves[q].filter,
                                            this.magSrf.MagneticStrength(i, j),
                                            alpha1,
                                            this.magSrf.Lambda(i, j, lambdaPole));
                                    }
                                }
                                k++;
                            }
                        }
                        s++;
                    }
                }
            }

            // scaling
            scale_koeff = aveFlux / (Math.PI * this.stokes_func("I", "V",
                                 this.magSrf.MagneticStrength(0, 0) * 1e-6,
                                            0.0, lambdaPole));
            for (int i = 0; i < a.Length; i++)
                for (int j = 0; j < a[i].Length; j++)
                    a[i][j] *= scale_koeff;

            double[] f = new double[allPhases];

            {
                int k = 0;
                for (int q = 0; q < this.curves.Length; q++)
                {
                    for (int p = 0; p < this.curves[q].phases.Length; p++)
                    {
                        f[k] = this.curves[q].value[p];
                        k++;
                    }
                }
            }

            double[][] aTa = new double[observablePatchesNumber][];
            for (int i = 0; i < aTa.Length; i++)
                aTa[i] = new double[observablePatchesNumber];

            double[] aTf = new double[observablePatchesNumber];

            double[] x = new double[observablePatchesNumber];

            double[][] aT = new double[observablePatchesNumber][];
            for (int i = 0; i < aT.Length; i++)
                aT[i] = new double[allPhases];

            for (int i = 0; i < aT.Length; i++)
            {
                for (int j = 0; j < aT[i].Length; j++)
                {
                    aT[i][j] = a[j][i];
                }
            }

            MathLib.Basic.ATrA(ref a, ref aTa);

            for (int i = 0; i < aTa.Length; i++)
                aTa[i][i] += regPar;

            MathLib.Basic.AMultB(ref aT, ref f, ref aTf);

            // Preparing input files for NNLS;
            StreamWriter sw = new StreamWriter(pathNNLSDir + "\\nnls_a.dat");
            sw.WriteLine("{0} {1}", a.Length + observablePatchesNumber, observablePatchesNumber);
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    sw.Write(string.Format("{0:0.000000000000000E000} ", a[i][j]).Replace(",", "."));
                }
                sw.Write("\r\n");
            }
            for (int i = 0; i < observablePatchesNumber; i++)
            {
                for (int j = 0; j < observablePatchesNumber; j++)
                {
                    if (i == j)
                        sw.Write(string.Format("{0:0.000E000} ", Math.Sqrt(regPar)).Replace(",", "."));
                    else
                        sw.Write(string.Format("{0:0.000E000} ", 0.0).Replace(",", "."));
                }
                sw.Write("\r\n");
            }
            sw.Close();

            sw = new StreamWriter(pathNNLSDir + "\\nnls_b.dat");
            sw.WriteLine("{0} {1}", f.Length + observablePatchesNumber, 1);
            for (int i = 0; i < a.Length; i++)
            {
                sw.Write(string.Format("{0:0.000000000000000E000}\r\n", f[i]).Replace(",", "."));
            }
            for (int i = 0; i < observablePatchesNumber; i++)
            {
                sw.Write(string.Format("{0:0.00000E000}\r\n", 0).Replace(",", "."));
            }
            sw.Close();

            // Run NNLS;
            Process pro = new Process();
            pro.StartInfo.FileName = pathNNLSDir + "\\NNLS.exe";
            pro.StartInfo.WorkingDirectory = pathNNLSDir;
            pro.StartInfo.Arguments = "d nnls_a.dat nnls_b.dat 5000";
            pro.Start();
            pro.WaitForExit();
            //

            StreamReader sr = new StreamReader(pathNNLSDir + "\\nnls_solution.dat");
            for (int i = 0; i < x.Length; i++)
                x[i] = double.Parse(sr.ReadLine().Replace(".", ","));
            sr.Close();
            
            //x = MathLib.LES_Solver.SolveWithGaussMethod(aTa, aTf);

            //MathLib.LES_Solver.ConvGradMethodPL(ref aTa, ref aTf, ref x, 1e-20);

            {
                int k = 0;
                for (int i = 0; i < observableLatBeltsNumber; i++)
                {
                    for (int j = 0; j < this.magSrfRes.Patches[i].Length; j++)
                    {
                        this.magSrfRes.SetDensity(i, j, x[k]);
                        k++;
                    }
                }
            }
        }

        public MagnetizedSurface ResultSurface { get { return this.magSrfRes; } }
        public double ScaleCoeff { get { return this.scale_koeff; } }
        public double SizeParameter { get { return this.sizePar; } }
    }
}
