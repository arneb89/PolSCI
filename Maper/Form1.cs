using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NPlot;
using System.Threading;
using MathLib;

namespace Maper
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();

            plotSM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_PrArea.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_PrArea.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_PrArea.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));
        }

        

        /********************************************************************************************/
        /****************************************STOKES IMAGING**************************************/
        /********************************************************************************************/

        StokesImaging.MagnetizedSurface magSrf = null;

        StokesImaging.ArcParsInitBox arcInitBox = null;

        StokesImaging.StokesModeller stokesModeller = null;

        StokesImaging.StokesParsProvider stokesProvider = null;

        private void btnSI_CreateSurface_Click(object sender, EventArgs e)
        {
            double magStr, betaOffset, lambdaOffset, inc, magStr2;
            int partFlag = 0;
            int n, m;
            try
            {
                magStr = double.Parse(txtSI_PolesMagFieldStr.Text.Replace(".", ",")) * 1e6;
                magStr2 = double.Parse(txtSI_B2.Text.Replace(".", ",")) * 1e6;
                betaOffset = double.Parse(txtSI_BetaOffset.Text.Replace(".", ",")) * Math.PI / 180;
                lambdaOffset = double.Parse(txtSI_LambdaOffset.Text.Replace(".", ",")) * Math.PI / 180;
                inc = double.Parse(txtSI_Inc.Text.Replace(".", ",")) * Math.PI / 180;
                n = int.Parse(txtSI_N.Text);
                m = int.Parse(txtSI_M.Text);
                if (cbSI_PartSimple.Checked) partFlag = 0;
                if (cbSI_PartEqAreas.Checked) partFlag = 1;
            }
            catch
            {
                return;
            }

            if (magStr == magStr2)
            {
                this.magSrf = new Maper.StokesImaging.MagnetizedSurface(inc, betaOffset, 
                    lambdaOffset, magStr, n, m, partFlag);
            }
            else
            {
                this.magSrf = new Maper.StokesImaging.MagnetizedSurface(inc, betaOffset, 
                    lambdaOffset, magStr, magStr2, n, m, partFlag);
            }
        }

        private void ShowArcPars(object sender, EventArgs e)
        {
            int spotNumber = lbSM_SpotStack.SelectedIndex;
            if (spotNumber == -1) return;

            this.txtSM_Phi1.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Phi2.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Theta1.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Theta2.LostFocus -= new EventHandler(ArcParsChanged);
            this.txtSM_Brightness.LostFocus -= new EventHandler(ArcParsChanged);

            txtSM_Phi1.Text = this.arcInitBox.spots[spotNumber].longitude1.ToString();
            txtSM_Phi2.Text = this.arcInitBox.spots[spotNumber].longitude2.ToString();
            txtSM_Theta1.Text = this.arcInitBox.spots[spotNumber].colatitude1.ToString();
            txtSM_Theta2.Text = this.arcInitBox.spots[spotNumber].colatitude2.ToString();
            txtSM_Brightness.Text = this.arcInitBox.spots[spotNumber].brightness.ToString();

            this.txtSM_Phi1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Phi2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Brightness.LostFocus += new EventHandler(ArcParsChanged);
        }

        private void ArcParsChanged(object sender, EventArgs e)
        {
            try
            {
                int spotNumber = lbSM_SpotStack.SelectedIndex;
                if (lbSM_SpotStack.SelectedIndex == -1) return;
                this.arcInitBox.spots[spotNumber].longitude1 = double.Parse(txtSM_Phi1.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].longitude2 = double.Parse(txtSM_Phi2.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].colatitude1 = double.Parse(txtSM_Theta1.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].colatitude2 = double.Parse(txtSM_Theta2.Text.Replace(".", ","));
                this.arcInitBox.spots[spotNumber].brightness = double.Parse(txtSM_Brightness.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }
        }

        private void btnSM_AddSpot_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null)
            {
                MessageBox.Show("You should build the surface first...", "Error...");
                return;
            }

            if (this.arcInitBox == null)
            {
                this.arcInitBox = new StokesImaging.ArcParsInitBox();
            }

            this.arcInitBox.AddSpot();

            lbSM_SpotStack.Items.Clear();

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                lbSM_SpotStack.Items.Add("Spot " + (i + 1).ToString());
            }

            this.lbSM_SpotStack.SelectedIndexChanged += new EventHandler(ShowArcPars);
            this.txtSM_Phi1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Phi2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta1.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Theta2.LostFocus += new EventHandler(ArcParsChanged);
            this.txtSM_Brightness.LostFocus += new EventHandler(ArcParsChanged);
        }

        private void btnSM_DelSpot_Click(object sender, EventArgs e)
        {
            if (this.arcInitBox == null) return;
            int num = lbSM_SpotStack.SelectedIndex;
            if (num != -1)
            {
                this.arcInitBox.DelSpot(num);
            }

            lbSM_SpotStack.Items.Clear();

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                lbSM_SpotStack.Items.Add("Spot " + (i + 1).ToString());
            }

            this.ShowArcPars(sender, e);
        }

        private void btnSM_GenerateStokesCurves_Click(object sender, EventArgs e)
        {
            int pointsCount;
            double poleOptDepth;
            double scale;

            if (this.magSrf == null) return;

            try
            {
                pointsCount = int.Parse(txtSM_PointsNumber.Text);
                poleOptDepth = double.Parse(txtSM_PoleOptDepth.Text);
                scale = double.Parse(txtSM_Scale.Text);
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }

            txtSM_Results.Text += "\r\nModel parameters:";
            txtSM_Results.Text += string.Format("\r\nInc[deg]: {0:00.000}",
                this.magSrf.InclinationOfRotationAxis * 180 / Math.PI);
            txtSM_Results.Text += string.Format("\r\nLat. Dipol Offset [deg]: {0:00.000}",
                this.magSrf.LatitudeDipolOffset * 180 / Math.PI);
            txtSM_Results.Text += string.Format("\r\nLong. Dipol Offset [deg]: {0:00.000}",
                this.magSrf.LongitudeDipolOffset * 180 / Math.PI);

            try
            {
                this.stokesProvider = new Maper.StokesImaging.StokesParsProvider(
                    @txtPathToStockesIGrid.Text,
                    @txtPathToStockesQGrid.Text,
                    @txtPathToStockesVGrid.Text
                    );
            }
            catch
            {
                MessageBox.Show("Cannot find some Stockes Grid file...", "Error...");
                txtSM_Results.Text += "\r\nOops... Some error was occured...";
                return;
            }

            //StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();

            double[] phases = new double[pointsCount];
            for (int i = 0; i < phases.Length; i++) phases[i] = i /(double)pointsCount;

            
            //this.stokesModeller = new Maper.StokesImaging.StokesModeller(
            //    /*stokesProvider.StokesI*/ wm85.GetStokesI,
            //    /*stokesProvider.StokesV*/ wm85.GetStokesV,
            //    /*stokesProvider.StokesQ*/ wm85.GetStokesQ,
            //    stokesProvider.StokesU,
            //    this.magSrf);

            this.stokesModeller = new Maper.StokesImaging.StokesModeller(
                stokesProvider.StokesI,
                stokesProvider.StokesV,
                stokesProvider.StokesQ,
                stokesProvider.StokesU,
                this.magSrf);


            this.stokesModeller.StartStokesCurvesModelling(phases, scale, poleOptDepth);
            
            LinePlot lpI = new LinePlot(this.stokesModeller.StokesI, phases);
            LinePlot lpV = new LinePlot(this.stokesModeller.StokesV, phases);
            LinePlot lpQ = new LinePlot(this.stokesModeller.StokesQ, phases);
            LinePlot lpU = new LinePlot(this.stokesModeller.StokesU, phases);

            plotSM_StokesI.Add(lpI);
            plotSM_StokesV.Add(lpV);
            plotSM_StokesQ.Add(lpQ);
            plotSM_StokesU.Add(lpU);

            plotSM_StokesI.Title = "Stokes I Curve";
            plotSM_StokesQ.Title = "Stokes Q Curve";
            plotSM_StokesU.Title = "Stokes U Curve";
            plotSM_StokesV.Title = "Stokes V Curve";

            plotSM_StokesI.XAxis1.Label = "Phase";
            plotSM_StokesQ.XAxis1.Label = "Phase";
            plotSM_StokesU.XAxis1.Label = "Phase";
            plotSM_StokesV.XAxis1.Label = "Phase";

            plotSM_StokesI.Refresh();
            plotSM_StokesV.Refresh();
            plotSM_StokesQ.Refresh();
            plotSM_StokesU.Refresh();

            txtSM_Results.Text += "\r\nError String: " + this.stokesModeller.ErrorString;
        }

        private void btnSM_AddSpotsToSurface_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null) return;

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                this.magSrf.AddRectSpot(this.arcInitBox.spots[i].colatitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].colatitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].brightness);
            }
        }

        private void btnSM_ClearUseMask_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null) return;

            this.magSrf.ClearBrightnessDensityArray();
        }

        private void btnSM_ShowMap_Click(object sender, EventArgs e)
        {
            if (this.magSrf == null)
            {
                MessageBox.Show("Surface was not created...", "Error...");
                return;
            }
            int part_flag=1;
            if (cbSI_PartSimple.Checked) part_flag = 0;

            StokesImaging.MagnetizedSurface boolSrf = new Maper.StokesImaging.MagnetizedSurface(0.0, 0.0, 0.0, 0.0,
                this.magSrf.NLat, this.magSrf.NLong, part_flag);

            if (this.arcInitBox == null) return;

            for (int i = 0; i < this.arcInitBox.SpotsNumber; i++)
            {
                boolSrf.AddRectSpot(this.arcInitBox.spots[i].colatitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].colatitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude1 * Math.PI / 180,
                    this.arcInitBox.spots[i].longitude2 * Math.PI / 180,
                    this.arcInitBox.spots[i].brightness);
            }

            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.White;
            svf.color1 = Color.Black;
            svf.Init(boolSrf.GetPatchCoordMas(), boolSrf.GetBrightnessDensityMas());
            svf.ShowDialog();
        }

        private void btnSI_ShowSurface_Click(object sender, EventArgs e)
        {
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(magSrf.GetPatchCoordMas(), magSrf.GetBMas());
            svf.ShowDialog();
        }

        private void btnSM_ShowThetaSurface_Click(object sender, EventArgs e)
        {
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.Blue;
            svf.color1 = Color.Red;
            svf.Init(magSrf.GetPatchCoordMas(), magSrf.GetThetaMas());
            svf.ShowDialog();
        }

        private void btnSM_ClearGraph_Click(object sender, EventArgs e)
        {
            plotSM_StokesI.Clear();
            plotSM_StokesQ.Clear();
            plotSM_StokesU.Clear();
            plotSM_StokesV.Clear();
            plotSM_PrArea.Clear();

            plotSM_StokesI.Refresh();
            plotSM_StokesQ.Refresh();
            plotSM_StokesU.Refresh();
            plotSM_StokesV.Refresh();
            plotSM_PrArea.Refresh();
        }

        private void btnSM_ShowStokesCurves_Click(object sender, EventArgs e)
        {
            if (rbSM_StokesView.Checked)
            {
                LinePlot lpV = new LinePlot(this.stokesModeller.StokesV, this.stokesModeller.PhasesV);
                LinePlot lpQ = new LinePlot(this.stokesModeller.StokesQ, this.stokesModeller.PhasesQ);
                LinePlot lpU = new LinePlot(this.stokesModeller.StokesU, this.stokesModeller.PhasesU);

                plotSM_StokesQ.Clear();
                plotSM_StokesU.Clear();
                plotSM_StokesV.Clear();

                plotSM_StokesV.Add(lpV);
                plotSM_StokesQ.Add(lpQ);
                plotSM_StokesU.Add(lpU);

                plotSM_StokesQ.Title = "Stokes Q Curve";
                plotSM_StokesU.Title = "Stokes U Curve";
                plotSM_StokesV.Title = "Stokes V Curve";
            }

            if (rbSM_SimpleView.Checked)
            {
                double[] circPol = new double[this.stokesModeller.StokesV.Length];
                
                for (int i = 0; i < circPol.Length; i++)
                {
                    if (this.stokesModeller.StokesI[i] == 0)
                    {
                        circPol[i] = 0;
                    }
                    else
                    {
                        circPol[i] = 100 * Math.Abs(this.stokesModeller.StokesV[i]) /
                            this.stokesModeller.StokesI[i];
                    }
                }

                double[] linPol = new double[this.stokesModeller.StokesV.Length];
                for (int i = 0; i < linPol.Length; i++)
                {
                    if (this.stokesModeller.StokesI[i] == 0)
                    {
                        linPol[i] = 0;
                    }
                    else
                    {
                        linPol[i] = 100 * Math.Sqrt(Math.Pow(this.stokesModeller.StokesQ[i], 2) +
                            Math.Pow(this.stokesModeller.StokesU[i], 2)) /
                            this.stokesModeller.StokesI[i];
                    }
                }

                double[] posAng = new double[this.stokesModeller.PhasesV.Length];
                for (int i = 0; i < posAng.Length; i++)
                {
                    if (this.stokesModeller.StokesQ[i] == 0)
                    {
                        posAng[i] = 90;
                    }
                    posAng[i] = 0.5 * Math.Atan(this.stokesModeller.StokesU[i] /
                        this.stokesModeller.StokesQ[i]) * 180 / Math.PI;

                    if (this.stokesModeller.StokesU[i] < 0 && this.stokesModeller.StokesQ[i] < 0)
                    {
                        posAng[i] = posAng[i]-90;
                    }
                    if (this.stokesModeller.StokesU[i] > 0 && this.stokesModeller.StokesQ[i] < 0)
                    {
                        posAng[i] = posAng[i] + 90;
                    }
                }


                LinePlot lpC = new LinePlot(circPol, this.stokesModeller.PhasesI);
                LinePlot lpP = new LinePlot(linPol, this.stokesModeller.PhasesV);
                LinePlot lpPos = new LinePlot(posAng, this.stokesModeller.PhasesV);

                plotSM_StokesQ.Clear();
                plotSM_StokesU.Clear();
                plotSM_StokesV.Clear();

                plotSM_StokesQ.Title = "Linear Polarization [%]";
                plotSM_StokesU.Title = "Position Angle [deg]";
                plotSM_StokesV.Title = "Circular Polarization [%]";

                plotSM_StokesQ.Add(lpP);
                plotSM_StokesU.Add(lpPos);
                plotSM_StokesV.Add(lpC);
            }

            LinePlot lpT = new LinePlot(this.stokesModeller.StokesI, this.stokesModeller.PhasesI);

            plotSM_StokesI.Clear();
            plotSM_StokesI.Title = "Total Flux";
            plotSM_StokesI.Add(lpT);

            plotSM_StokesI.XAxis1.Label = "Phase";
            plotSM_StokesQ.XAxis1.Label = "Phase";
            plotSM_StokesU.XAxis1.Label = "Phase";
            plotSM_StokesV.XAxis1.Label = "Phase";

            plotSM_StokesI.Refresh();
            plotSM_StokesV.Refresh();
            plotSM_StokesQ.Refresh();
            plotSM_StokesU.Refresh();
        }

        private void btnSM_InterpWM85_Click(object sender, EventArgs e)
        {
            double nuratio;
            double optdepth;
            double theta;

            try
            {
                nuratio = double.Parse(txtSM_NuRatioTest.Text.Replace(".", ","));
                optdepth = double.Parse(txtSM_LambdaTest.Text.Replace(".", ","));
                theta = double.Parse(txtSM_ThetaTest.Text.Replace(".", ",")) * Math.PI / 180;
            }
            catch
            {
                MessageBox.Show("Wrong format of input data...", "Error...");
                return;
            }
            
            StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();

            double[] xGrid = null, fGrid = null, xInterp = null, yInterp = null;
            PointPlot pp = new PointPlot();
            LinePlot lp = new LinePlot();
            xInterp = new double[100];
            yInterp = new double[100];

            if (rbSM_GridTest_Total.Checked && rbSM_GridTest_NuRatio.Checked)
            {
                xGrid = new double[wm85.GetThetaSet.Length];
                fGrid = new double[wm85.GetThetaSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetThetaSet[0] + i * (wm85.GetThetaSet[wm85.GetThetaSet.Length - 1] - wm85.GetThetaSet[0]) / (double)xInterp.Length;
                    xInterp[i] = xInterp[i] * 180 / Math.PI;
                    yInterp[i] = wm85.InterpTotal(nuratio, xInterp[i] * Math.PI / 180, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetThetaSet[i];
                    xGrid[i] = xGrid[i] * 180 / Math.PI;
                    fGrid[i] = wm85.InterpTotal(nuratio, xGrid[i] * Math.PI / 180, optdepth);
                }
            }
            if (rbSM_GridTest_Total.Checked && rbSM_GridTest_FixTheta.Checked)
            {
                xGrid = new double[wm85.GetNuRatSet.Length];
                fGrid = new double[wm85.GetNuRatSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetNuRatSet[0] + i * (wm85.GetNuRatSet[wm85.GetNuRatSet.Length - 1] - wm85.GetNuRatSet[0]) / (double)xInterp.Length;
                    yInterp[i] = wm85.InterpTotal(xInterp[i], theta, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetNuRatSet[i];
                    fGrid[i] = wm85.InterpTotal(xGrid[i], theta, optdepth);
                }
            }
            if (rbSM_GridTest_Linear.Checked && rbSM_GridTest_NuRatio.Checked)
            {
                xGrid = new double[wm85.GetThetaSet.Length];
                fGrid = new double[wm85.GetThetaSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetThetaSet[0] + i * (wm85.GetThetaSet[wm85.GetThetaSet.Length - 1] - wm85.GetThetaSet[0]) / (double)xInterp.Length;
                    xInterp[i] = xInterp[i] * 180 / Math.PI;
                    yInterp[i] = wm85.InterpLinearPart(nuratio, xInterp[i] * Math.PI / 180, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetThetaSet[i];
                    xGrid[i] = xGrid[i] * 180 / Math.PI;
                    fGrid[i] = wm85.InterpLinearPart(nuratio, xGrid[i] * Math.PI / 180, optdepth);
                }
            }
            if (rbSM_GridTest_Linear.Checked && rbSM_GridTest_FixTheta.Checked)
            {
                xGrid = new double[wm85.GetNuRatSet.Length];
                fGrid = new double[wm85.GetNuRatSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetNuRatSet[0] + i * (wm85.GetNuRatSet[wm85.GetNuRatSet.Length - 1] - wm85.GetNuRatSet[0]) / (double)xInterp.Length;
                    yInterp[i] = wm85.InterpLinearPart(xInterp[i], theta, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetNuRatSet[i];
                    fGrid[i] = wm85.InterpLinearPart(xGrid[i], theta, optdepth);
                }
            }
            if (rbSM_GridTest_Circular.Checked && rbSM_GridTest_NuRatio.Checked)
            {
                xGrid = new double[wm85.GetThetaSet.Length];
                fGrid = new double[wm85.GetThetaSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetThetaSet[0] + i * (wm85.GetThetaSet[wm85.GetThetaSet.Length - 1] - wm85.GetThetaSet[0]) / (double)xInterp.Length;
                    xInterp[i] = xInterp[i] * 180 / Math.PI;
                    yInterp[i] = wm85.GetStokesV(14.0e6, xInterp[i] * Math.PI / 180, 1e7);
                    //yInterp[i] = wm85.InterpCircularPart(nuratio, xInterp[i] * Math.PI / 180, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetThetaSet[i];
                    xGrid[i] = xGrid[i] * 180 / Math.PI;
                    fGrid[i] = wm85.InterpCircularPart(nuratio, xGrid[i] * Math.PI / 180, optdepth);
                }
            }
            if (rbSM_GridTest_Circular.Checked && rbSM_GridTest_FixTheta.Checked)
            {
                xGrid = new double[wm85.GetNuRatSet.Length];
                fGrid = new double[wm85.GetNuRatSet.Length];
                for (int i = 0; i < xInterp.Length; i++)
                {
                    xInterp[i] = wm85.GetNuRatSet[0] + i * (wm85.GetNuRatSet[wm85.GetNuRatSet.Length - 1] - wm85.GetNuRatSet[0]) / (double)xInterp.Length;
                    yInterp[i] = wm85.InterpCircularPart(xInterp[i], theta, optdepth);
                }
                for (int i = 0; i < xGrid.Length; i++)
                {
                    xGrid[i] = wm85.GetNuRatSet[i];
                    fGrid[i] = wm85.InterpCircularPart(xGrid[i], theta, optdepth);
                }
            }

            pp = new PointPlot(new Marker(Marker.MarkerType.FilledCircle, 3, Color.Black));
            pp.OrdinateData = fGrid;
            pp.AbscissaData = xGrid;
            lp = new LinePlot(yInterp, xInterp);

            plotSM_TestGrid.Add(pp);
            plotSM_TestGrid.Add(lp);

            plotSM_TestGrid.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotSM_TestGrid.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotSM_TestGrid.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotSM_TestGrid.Refresh();
        }

        private void btnSM_ClearGraphWM85_Click(object sender, EventArgs e)
        {
            plotSM_TestGrid.Clear();
            plotSM_TestGrid.Refresh();
        }

        private void btnSM_SaveCurve_Click(object sender, EventArgs e)
        {
            if (this.stokesModeller == null) return;

            saveFileDialog1.ShowDialog();

            string path = saveFileDialog1.FileName;
            
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(path);
            }
            catch
            {
                return;
            }

            if (rbSM_SaveAll.Checked)
            {
                for (int i = 0; i < this.stokesModeller.PhasesI.Length; i++)
                {
                    sw.WriteLine(string.Format(
                        "{0:000.000}\t{1:0.00000E000}\t{2:0.00000E000}\t{3:0.00000E000}\t{4:0.00000E000}",
                        this.stokesModeller.PhasesI[i],
                        this.stokesModeller.StokesI[i],
                        this.stokesModeller.StokesV[i],
                        this.stokesModeller.StokesQ[i],
                        this.stokesModeller.StokesU[i]).Replace(",", "."));
                }
            }
            if (rbSM_SaveI.Checked)
            {
                if (this.stokesModeller.StokesI == null) return;
                sw.WriteLine("I {0} {1}", "V", "I");
                sw.WriteLine("S {0}", this.stokesModeller.StokesI.Length);
                for (int i = 0; i < this.stokesModeller.PhasesI.Length; i++)
                {
                    sw.WriteLine("{0:0.00000} {1:0.00000E000}",
                        this.stokesModeller.PhasesI[i], this.stokesModeller.StokesI[i]);
                }
            }
            if (rbSM_SaveQ.Checked)
            {
                if (this.stokesModeller.StokesQ == null) return;
                sw.WriteLine("I {0} {1}", "V", "Q");
                sw.WriteLine("S {0}", this.stokesModeller.StokesQ.Length);
                for (int i = 0; i < this.stokesModeller.PhasesQ.Length; i++)
                {
                    sw.WriteLine("{0:0.00000} {1:0.00000E000}",
                        this.stokesModeller.PhasesQ[i], this.stokesModeller.StokesQ[i]);
                }
            }
            if (rbSM_SaveU.Checked)
            {
                if (this.stokesModeller.StokesU == null) return;
                sw.WriteLine("I {0} {1}", "V", "U");
                sw.WriteLine("S {0}", this.stokesModeller.StokesU.Length);
                for (int i = 0; i < this.stokesModeller.PhasesU.Length; i++)
                {
                    sw.WriteLine("{0:0.00000} {1:0.00000E000}",
                        this.stokesModeller.PhasesU[i], this.stokesModeller.StokesU[i]);
                }
            }
            if (rbSM_SaveV.Checked)
            {
                if (this.stokesModeller.StokesV == null) return;
                sw.WriteLine("I {0} {1}", "V", "V");
                sw.WriteLine("S {0}", this.stokesModeller.StokesV.Length);
                for (int i = 0; i < this.stokesModeller.PhasesV.Length; i++)
                {
                    sw.WriteLine("{0:0.00000} {1:0.00000E000}",
                        this.stokesModeller.PhasesV[i], this.stokesModeller.StokesV[i]);
                }
            }
            sw.Flush();
            sw.Close();
        }

        private void btnSMTest_Plot0_Click(object sender, EventArgs e)
        {
            this.stokesProvider = new Maper.StokesImaging.StokesParsProvider(
                    @txtPathToStockesIGrid.Text,
                    @txtPathToStockesQGrid.Text,
                    @txtPathToStockesVGrid.Text
                    );
            
        }

        /*************************************************************************************************/
        /*************************************************************************************************/

        StokesImaging.StokesCurvesBox stokesCurvesBox = null;
        StokesImaging.MagnetizedSurface magSrfRes = null;
        StokesImaging.StokesParsProvider spp_ism = null;
        StokesImaging.StokesMapper sm;

        private void ShowStokesCurvePars(object sender, EventArgs e)
        {
            int curveNumber = lbISM_StokesCurves.SelectedIndex;
            if (curveNumber == -1) return;

            this.txtISM_Sigma.LostFocus -= new EventHandler(StokesCurveParsChanged);

            txtISM_Filter.Text = this.stokesCurvesBox.StokesCurves[curveNumber].filter;
            txtISM_Type.Text = this.stokesCurvesBox.StokesCurves[curveNumber].type;
            txtISM_Sigma.Text = this.stokesCurvesBox.StokesCurves[curveNumber].sigma.ToString();

            this.txtISM_Sigma.LostFocus += new EventHandler(StokesCurveParsChanged);
        }

        void StokesCurveParsChanged(object sender, EventArgs e)
        {
            try
            {
                int curveNumber = lbISM_StokesCurves.SelectedIndex;
                if (curveNumber == -1) return;
                this.stokesCurvesBox.StokesCurves[curveNumber].sigma = 
                    double.Parse(txtISM_Sigma.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }      
        }

        private void btnISM_AddStokesCurve_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (this.stokesCurvesBox == null)
            {
                this.stokesCurvesBox = new Maper.StokesImaging.StokesCurvesBox();
            }
            string path = openFileDialog1.FileName;
            if (path == null) return;
            //try
            {
                this.stokesCurvesBox.AddStokesCurve(path);
            }
            //catch
            {
            //    MessageBox.Show("No such file or error in file structure...", "Error");
            }

            lbISM_StokesCurves.Items.Clear();

            for (int i = 0; i < this.stokesCurvesBox.StokesCurvesNumber; i++)
            {
                lbISM_StokesCurves.Items.Add("Stokes Curve # " + (i + 1).ToString());
            }

            this.lbISM_StokesCurves.SelectedIndexChanged += new EventHandler(ShowStokesCurvePars);
            this.txtISM_Sigma.LostFocus += new EventHandler(StokesCurveParsChanged);
        }

        private void btnISM_DeleteStokesCurve_Click(object sender, EventArgs e)
        {
            if (this.stokesCurvesBox == null) return;
            if (this.stokesCurvesBox.StokesCurvesNumber == 0) return;

            int num = lbISM_StokesCurves.SelectedIndex;
            if (num != -1)
            {
                this.stokesCurvesBox.DeleteStokesCurve(num);
            }

            lbISM_StokesCurves.Items.Clear();

            for (int i = 0; i < this.stokesCurvesBox.StokesCurvesNumber; i++)
            {
                lbISM_StokesCurves.Items.Add("Stokes Curve # " + (i + 1).ToString());
            }

            this.ShowStokesCurvePars(sender, e);
        }

        private void btnISM_StartMapping_Click(object sender, EventArgs e)
        {
            double polesMagStr;
            double polesLong;
            double polesCoLat;
            double inc;
            double regPar;
            double logL;
            int n, m;
            
            try
            {
                polesMagStr = double.Parse(txtISM_PolesMagStr.Text.Replace(".", ",")) * 1e6;
                polesLong = double.Parse(txtISM_PolesLongitude.Text.Replace(".", ",")) * Math.PI / 180;
                polesCoLat = double.Parse(txtISM_PolesColatitude.Text.Replace(".", ",")) * Math.PI / 180;
                inc = double.Parse(txtISM_Inclination.Text.Replace(".", ",")) * Math.PI / 180;
                regPar = double.Parse(txtISM_RegulParameter.Text.Replace(".", ","));
                n = int.Parse(txtISM_LatBeltsNumber.Text);
                m = int.Parse(txtISM_LongBeltsNumber.Text);
                logL = double.Parse(txtLogL.Text.Replace(".", ","));
            }
            catch
            {
                MessageBox.Show("Wrong format of input data...", "Error...");
                return;
            }

            //StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();


            spp_ism = new Maper.StokesImaging.StokesParsProvider(
                txtISM_IPath.Text,
                txtISM_QPath.Text,
                txtISM_VPath.Text
                );

            StokesImaging.MagnetizedSurface srf = new Maper.StokesImaging.MagnetizedSurface(inc, polesCoLat, polesLong,
                polesMagStr, n, m, 1);

            sm = new Maper.StokesImaging.StokesMapper(srf,
                spp_ism.GetStokesPars, this.stokesCurvesBox.StokesCurves);

            txtIS_Results.AppendText("\r\nStart imaging procedure...");
            MathLib.Counter counter = new MathLib.Counter();
            counter.Start();
            sm.StartMapping(regPar, Math.Pow(10, logL), Application.StartupPath);
            counter.Stop();
            txtIS_Results.AppendText(
                string.Format("\r\nDone!\r\nComputings duration is: {0} sec", 
                counter.TotalSeconds.ToString()));

            this.magSrfRes = sm.ResultSurface;
        }

        private void btnISM_ShowStokesCurve_Click(object sender, EventArgs e)
        {
            int curveNumber = lbISM_StokesCurves.SelectedIndex;
            if (curveNumber == -1) return;

            PointPlot pp = new PointPlot(new Marker(Marker.MarkerType.FilledCircle, 3, Color.Black));
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "I")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesI.Add(pp);
                plotISM_StokesI.Refresh();
            }
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "V")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesV.Add(pp);
                plotISM_StokesV.Refresh();
            }
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "Q")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesQ.Add(pp);
                plotISM_StokesQ.Refresh();
            }
            if (this.stokesCurvesBox.StokesCurves[curveNumber].type == "U")
            {
                pp.AbscissaData = this.stokesCurvesBox.StokesCurves[curveNumber].phases;
                pp.OrdinateData = this.stokesCurvesBox.StokesCurves[curveNumber].value;
                plotISM_StokesU.Add(pp);
                plotISM_StokesU.Refresh();
            }
        }

        private void btnISM_ModelCurves_Click(object sender, EventArgs e)
        {
            //StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();
            StokesImaging.StokesModeller modeller = new Maper.StokesImaging.StokesModeller(
                spp_ism.StokesI /*wm85.GetStokesI*/,
                spp_ism.StokesV /*wm85.GetStokesV*/,
                spp_ism.StokesQ /*wm85.GetStokesQ*/,
                spp_ism.StokesU,
                sm.ResultSurface);

            int pointsCount = 100;
            double[] phases = new double[pointsCount];
            for (int i = 0; i < phases.Length; i++) phases[i] = i / (double)pointsCount;

            txtIS_Results.AppendText("\r\nStart stokes curve modelling...");
            modeller.StartStokesCurvesModelling(phases, sm.ScaleCoeff, sm.SizeParameter);
            txtIS_Results.AppendText("\r\nDone!");

            StreamWriter sw_i = new StreamWriter(Application.StartupPath + "\\stokes_i_reconstructed.dat");
            StreamWriter sw_q = new StreamWriter(Application.StartupPath + "\\stokes_q_reconstructed.dat");
            StreamWriter sw_v = new StreamWriter(Application.StartupPath + "\\stokes_v_reconstructed.dat");
            StreamWriter sw_u = new StreamWriter(Application.StartupPath + "\\stokes_u_reconstructed.dat");

            for (int i = 0; i < modeller.StokesI.Length; i++)
            {
                sw_i.WriteLine("{0}\t{1}", phases[i], modeller.StokesI[i]);
                sw_q.WriteLine("{0}\t{1}", phases[i], modeller.StokesQ[i]);
                sw_v.WriteLine("{0}\t{1}", phases[i], modeller.StokesV[i]);
                sw_u.WriteLine("{0}\t{1}", phases[i], modeller.StokesU[i]);
            }

            sw_i.Flush();
            sw_q.Flush();
            sw_v.Flush();
            sw_u.Flush();

            sw_i.Close();
            sw_q.Close();
            sw_v.Close();
            sw_u.Close();


            LinePlot lpI = new LinePlot(modeller.StokesI, phases);
            LinePlot lpV = new LinePlot(modeller.StokesV, phases);
            LinePlot lpQ = new LinePlot(modeller.StokesQ, phases);
            LinePlot lpU = new LinePlot(modeller.StokesU, phases);

            plotISM_StokesI.Add(lpI);
            plotISM_StokesV.Add(lpV);
            plotISM_StokesQ.Add(lpQ);
            plotISM_StokesU.Add(lpU);

            plotISM_StokesI.Title = "Stokes I Curve";
            plotISM_StokesQ.Title = "Stokes Q Curve";
            plotISM_StokesU.Title = "Stokes U Curve";
            plotISM_StokesV.Title = "Stokes V Curve";

            plotISM_StokesI.XAxis1.Label = "Phase";
            plotISM_StokesQ.XAxis1.Label = "Phase";
            plotISM_StokesU.XAxis1.Label = "Phase";
            plotISM_StokesV.XAxis1.Label = "Phase";

            plotISM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotISM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotISM_StokesI.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotISM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotISM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotISM_StokesV.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotISM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotISM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotISM_StokesQ.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotISM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag());
            plotISM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.VerticalDrag());
            plotISM_StokesU.AddInteraction(new NPlot.Windows.PlotSurface2D.Interactions.AxisDrag(true));

            plotISM_StokesI.Refresh();
            plotISM_StokesQ.Refresh();
            plotISM_StokesU.Refresh();
            plotISM_StokesV.Refresh();
        }

        private void btmISM_ShowMap_Click(object sender, EventArgs e)
        {
            SurfaceViewerForm svf = new SurfaceViewerForm();
            svf.color0 = Color.White;
            svf.color1 = Color.Black;
            svf.Init(this.magSrfRes.GetPatchCoordMas(), this.magSrfRes.GetBrightnessDensityMas());
            svf.ShowDialog();
        }

        private void btnISM_StartFit_Click(object sender, EventArgs e)
        {
            //double polesMagStr;
            //double polesLong;
            //double polesCoLat;
            //double inc;
            //int n, m;
            //try
            //{
            //    polesMagStr = double.Parse(txtISM_PolesMagStr.Text.Replace(".", ",")) * 1e6;
            //    polesLong = double.Parse(txtISM_PolesLongitude.Text.Replace(".", ",")) * Math.PI / 180;
            //    polesCoLat = double.Parse(txtISM_PolesColatitude.Text.Replace(".", ",")) * Math.PI / 180;
            //    inc = double.Parse(txtISM_Inclination.Text.Replace(".", ",")) * Math.PI / 180;
            //    n = int.Parse(txtISM_LatBeltsNumber.Text);
            //    m = int.Parse(txtISM_LongBeltsNumber.Text);
            //}
            //catch
            //{
            //    MessageBox.Show("Wrong format of input data...", "Error...");
            //    return;
            //}

            //double longit1 = double.Parse(txtISM_Long1.Text.Replace(".", ","));
            //double longit2 = double.Parse(txtISM_Long2.Text.Replace(".", ","));
            //double longNum = int.Parse(txtISM_LongNumber.Text);
            //double colat1 = double.Parse(txtISM_Colat1.Text.Replace(".", ","));
            //double colat2 = double.Parse(txtISM_Colat2.Text.Replace(".", ","));
            //double colatNum = double.Parse(txtISM_ColatNumber.Text);

            //double[] longMas = new double[longNum];
            //double[] colatMas = new double[colatNum];
            //for (int i = 0; i < longMas.Length; i++)
            //{
            //    longMas[i] = longit1 + i * (longit2 - longit1) / longNum;
            //}
            //for (int i = 0; i < colatMas.Length; i++)
            //{
            //    colatMas[i] = colat1 + i * (colat2 - colat1) / colatNum;
            //}

            //StokesImaging.MagnetizedSurface msrf = new Maper.StokesImaging.MagnetizedSurface(inc, polesCoLat,
            //    polesLong, polesMagStr, n, m);

            //StokesImaging.WM85Interpolator wm85 = new Maper.StokesImaging.WM85Interpolator();

            //StokesImaging.StokesModeller modeller = new Maper.StokesImaging.StokesModeller(
            //    /*stokesProvider.StokesI*/ wm85.GetStokesI,
            //    /*stokesProvider.StokesV*/ wm85.GetStokesV,
            //    /*stokesProvider.StokesQ*/ wm85.GetStokesQ,
            //    wm85.GetStokesU,
            //    magSrfRes);
            //double[] modfluxesI;
            //double[] modfluxesV;

            //for (int l1 = 0; l1 < longMas.Length; l1++)
            //{
            //    for (int l2 = l1; l2 < longMas.Length; l2++)
            //    {
            //        for (int c1 = 0; c1 < colatMas.Length; c1++)
            //        {
            //            for (int c2 = c1; c2 < colatMas.Length; c2++)
            //            {
            //                msrf.AddRectSpot(colat1, colat2, longit1, longit2, 10);
            //                modeller.StartStokesCurvesModelling(this.stokesCurvesBox.StokesCurves[0].phases, 1.0);
            //                modfluxesI=modeller.StokesI;
            //                double sumKhi = 0;
            //                for (int i = 0; i < modfluxesI.Length; i++)
            //                {
            //                }

            //                msrf.ClearBrightnessDensityArray();
            //            }
            //        }
            //    }
            //}

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int pointsCount;
            double poleOptDepth;
            double scale;

  

            try
            {
                pointsCount = int.Parse(txtSM_PointsNumber.Text);
                poleOptDepth = double.Parse(txtSM_PoleOptDepth.Text);
                scale = double.Parse(txtSM_Scale.Text);
            }
            catch
            {
                MessageBox.Show("Wrong format...", "Error...");
                return;
            }

            

            ///////////////////////////////////////////////////////////////////////////////////////

            

            try
            {
                this.stokesProvider = new Maper.StokesImaging.StokesParsProvider(
                    @txtPathToStockesIGrid.Text,
                    @txtPathToStockesQGrid.Text,
                    @txtPathToStockesVGrid.Text
                    );
            }
            catch
            {
                MessageBox.Show("Cannot find some Stockes Grid file...", "Error...");
                txtSM_Results.Text += "\r\nOops... Some error was occured...";
                return;
            }

            double[] phases = new double[pointsCount];
            for (int i = 0; i < phases.Length; i++) phases[i] = i / (double)pointsCount;

            StreamWriter sw = new StreamWriter("D:\\out.dat");

            for(double bbg=0; bbg<=38.0; bbg=bbg+0.5)
            {
                double mm1 = 49 * 1e6, mm2 = 51 * 1e6;
                mm1 = 43.31554 - 0.25286 * bbg + 0.00373 * bbg * bbg; // 39
                mm2 = 57.93482 - 0.41099 * bbg + 0.00617 * bbg * bbg; // 51
                mm1 = mm1 * 1e6;
                mm2 = mm2 * 1e6;
                magSrf = new Maper.StokesImaging.MagnetizedSurface(79 * Math.PI / 180, bbg * Math.PI / 180, 80 * Math.PI / 180, mm1, mm2, 360, 720, 0);
                this.stokesModeller = new Maper.StokesImaging.StokesModeller(
                stokesProvider.StokesI,
                stokesProvider.StokesV,
                stokesProvider.StokesQ,
                stokesProvider.StokesU,
                this.magSrf);

                this.magSrf.AddRectSpot((38-bbg) * Math.PI / 180, //!
                    (39-bbg) * Math.PI / 180,                     //!
                    179 * Math.PI / 180,
                    181 * Math.PI / 180,
                    1.0);

                for (double l = 1; l <= 6; l = l + 0.1)
                {
                    this.stokesModeller.StartStokesCurvesModelling(phases, scale, Math.Pow(10,l)*39e6/mm1);
                    double[] sti = stokesModeller.StokesI;
                    double[] stv = stokesModeller.StokesV;
                    sw.Write(string.Format("{0}\t{1}\t", bbg, l).Replace(",", "."));
                    for (int k = 0; k < sti.Length; k++) sw.Write(string.Format("{0}\t", sti[k]).Replace(",", "."));
                    for (int k = 0; k < stv.Length; k++) sw.Write(string.Format("{0}\t", stv[k]).Replace(",", "."));
                    sw.Write("\r\n");
                }
            }
            sw.Close();
            txtSM_Results.Text += "\r\nError String: " + this.stokesModeller.ErrorString;
        }

        /*********************************************************************************************************/
        /*********************************************************************************************************/

    }
}
