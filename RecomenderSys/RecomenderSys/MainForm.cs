using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RecomenderSys.Models;
using RecomenderSys.Services;

namespace RecomenderSys
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            mapReduceService = new MapReduceService(datasetPath, profilesPath);
            Films = mapReduceService.Films;
            UserProfiles = mapReduceService.UserProfiles;
            gridFilms.DataSource = Films;

        }

        private string datasetPath = Application.StartupPath + "\\dataset";

        private string profilesPath = Application.StartupPath + "\\userprofile";

        public List<Entity> Films { get; set; }

        public List<UserProfile> UserProfiles { get; set; }

        public List<Entity> Recommendation { get; set; } 

        public List<Entity> MapReduceResult { get; set; } 

        private MapReduceService mapReduceService { get; set; }

        public Entity SelectedFilm
        {
            get
            {
                if(gridFilms.SelectedRows.Any())
                    return Films[gridFilms.SelectedRows[0].Index];
                return Films[0];
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            
            var service = new RecomenderService(datasetPath);
            //service.DataSetFilePath = datasetPath;

            //var files = service.FindTags(service.DataSetFilePath);
            //service.InputFiles = files;
            Recommendation = service.Process(SelectedFilm);
            Application.DoEvents();
            gridRecommend.DataSource = Recommendation;
            
            //var tags = service.FindTags("");

            //var result = new List<Models.Entity>();

            //foreach (var tag in tags)
            //{
            //    //var weights = service.WeightWords(tag.Words);
            //    //weighted_tags.AddRange(weights);
            //    result.Add(service.WeightWords(tag.Words));
            //}



        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.DoEvents();
            var service = new MapReduceService(datasetPath, profilesPath);
            MapReduceResult = service.MapReduce(SelectedFilm, 3);
            Application.DoEvents();
            gridMapReduce.DataSource = MapReduceResult;
        }

        private void gridRecommend_CellFormatting(object sender, Telerik.WinControls.UI.CellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                e.CellElement.Text = Convert.ToDecimal(e.CellElement.Value.ToString()).ToString("0.0000");
            }
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            Cursor = Cursors.WaitCursor;
            radButton1.Enabled = false;
            radLabel2.Text = "Results for " + SelectedFilm.Title;
            button1_Click(sender, e);
            Application.DoEvents();
            button2_Click(sender, e);
            Application.DoEvents();
            lblStatus.Text = string.Format("Results : Content Based ({0}) items, Collabrative Filtering ({1}) items",
                Recommendation.Count, MapReduceResult.Count);
            radButton1.Enabled = true;
            Cursor = Cursors.Default;
            progressBar.Visible = false;
        }
    }
}
