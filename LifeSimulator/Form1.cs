using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeSimulator
{
    public partial class Form1 : Form
    {
        public List<SimEntity> entities { get; set; }
        public List<Resource> resources { get; set; }

        int sim = 21;

        int yrCount = 0;
        Random rand;

        public Form1()
        {
            InitializeComponent();
        }

        private void picViewport_Click(object sender, EventArgs e)
        {

        }

        private void picViewport_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            foreach (SimEntity entity in entities)
            {
                entity.Draw(e.Graphics);
            }
            foreach (Resource rsc in resources)
            {
                rsc.Draw(e.Graphics);
            }

            if(entities.Count == 1)
            {
                string settings = JsonConvert.SerializeObject(entities[0]);
                File.WriteAllText(sim.ToString() + ".json", settings);
                Application.Exit();
            }

            if(entities.Count > 0)
            {
                Text = $"Avg Size : {entities.Average(x => x.Size)}, Avg Age :" +
                        $" {(int)entities.Average(x => x.Age)}, Avg Energy :" +
                        $" {(int)entities.Average(x => x.Energy)}";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            entities = new List<SimEntity>();
            resources = new List<Resource>();

            rand = new Random(Guid.NewGuid().GetHashCode());

            for(int i=0;i<50;i++)
            {
                entities.Add(new SimEntity(rand)
                {
                    Location = new Point(Width / 2 + rand.Next(-250, 250), Height / 2 + rand.Next(-250, 250)),
                    Size = 4,
                    MaxAge = 255,
                    Energy = 100
                });
            }

            for(int i=0;i<400;i++)
            {
                resources.Add(new Resource() { Location = new Point(Width / 2 + rand.Next(-250, 250), Height / 2 + rand.Next(-250, 250)), Size = 3 });
            }

            for (int i = 0; i < 150; i++)
            {
                resources.Add(new Resource() { Location = new Point(Width / 4 + rand.Next(-50, 50), Height / 4 + rand.Next(-50, 50)), Size = 3 });
            }

            for (int i = 0; i < 200; i++)
            {
                resources.Add(new Resource() { Location = new Point(Width * 5 / 7 + rand.Next(-60, 60), Height / 4 + rand.Next(-60, 60)), Size = 3 });
            }

            for (int i = 0; i < 180; i++)
            {
                resources.Add(new Resource() { Location = new Point(Width * 5 / 7 + rand.Next(-45, 45), Height  * 4 / 7 + rand.Next(-45, 45)), Size = 4 });
            }

            tmrSim.Start();
        }

        private List<float> CalculateInputs(int x, int y)
        {
            float n, s, e, w;
            n = resources.Where(r => r.Location.Y < y).Count();
            s = resources.Where(r => r.Location.Y > y).Count();
            e = resources.Where(r => r.Location.X > x).Count();
            w = resources.Where(r => r.Location.X < x).Count();

            n -= entities.Where(r => r.Location.Y < y).Count();
            s -= entities.Where(r => r.Location.Y > y).Count();
            e -= entities.Where(r => r.Location.X > x).Count();
            w -= entities.Where(r => r.Location.X < x).Count();

            return new List<float>() { n / (n + s + e + w), s / (n + s + e + w), e / (n + s + e + w), w / (n + s + e + w) };
        }

        private void tmrSim_Tick(object sender, EventArgs e)
        {
            yrCount++;
            if(yrCount > 0 && yrCount % 2 == 0)
            {
                yrCount = 0;
                foreach (SimEntity entity in entities)
                {
                    entity.Inputs = CalculateInputs(entity.Location.X, entity.Location.Y);
                    entity.IncrementAge();
                    entity.NextAction();
                    picViewport.Refresh();
                }
                entities.RemoveAll(x => x.MarkForRemoval == true);

                for(int i=0;i<entities.Count;i++)
                {
                    var consumption = resources.Where(x => Math.Abs(x.Location.X - entities[i].Location.X) <= 10
                    && Math.Abs(x.Location.Y - entities[i].Location.Y) <= 10).Sum(x => x.Size);

                    resources.RemoveAll(x => Math.Abs(x.Location.X - entities[i].Location.X) <= 10
                    && Math.Abs(x.Location.Y - entities[i].Location.Y) <= 10);

                    if(consumption > 0)
                    {
                        entities[i].Energy += consumption * 2;
                        entities[i].RewardAction(0.01f);
                    }
                    else
                    {
                        entities[i].RewardAction(-0.001f);
                    }
                }

                var creatingEnts = entities.Where(x => x.ExtFlag == 1).ToList();
                foreach(var ent in creatingEnts)
                {
                    entities.Add(ent.CreateNewEntity(rand));
                    ent.ExtFlag = 0;
                }
                for(int i=0;i<creatingEnts.Count();i++)
                {
                    entities.Add(creatingEnts[i].CreateNewEntity(rand));
                    creatingEnts[i].ExtFlag = 0;
                }

                var predatorEnts = entities.Where(x => x.ExtFlag == 2).ToList();
                foreach (var ent in predatorEnts)
                {
                    for(int i=0;i<entities.Count;i++)
                    {
                        if(entities[i].GetSimiliarity(ent.actionVector) > ent.actionVector[1])
                        {
                            if (entities[i].Size < ent.Size && entities[i].Energy < ent.Energy && Math.Abs(entities[i].Age - ent.Age) < 10)
                            {
                                var dist = (ent.Location.X - entities[i].Location.X) * (ent.Location.X - entities[i].Location.X) +
                                        (ent.Location.Y - entities[i].Location.Y) * (ent.Location.Y - entities[i].Location.Y);
                                if (dist < 10)
                                {
                                    ent.BurnEnergy(5);
                                    entities[i].BurnEnergy(5);
                                    entities[i].MarkForRemoval = true;

                                    ent.Energy += entities[i].Energy;
                                    ent.RewardAction(0.01f);
                                }
                            }
                            else if (Math.Abs(entities[i].Age - ent.Age) > 10)
                            {
                                if (entities[i].Age < ent.Age && entities[i].Energy < ent.Energy && entities[i].Age > 50)
                                {
                                    var dist = (ent.Location.X - entities[i].Location.X) * (ent.Location.X - entities[i].Location.X) +
                                        (ent.Location.Y - entities[i].Location.Y) * (ent.Location.Y - entities[i].Location.Y);
                                    if (dist < 10)
                                    {
                                        ent.BurnEnergy(5);
                                        entities[i].BurnEnergy(5);
                                        ent.MarkForRemoval = true;

                                        entities[i].Energy += ent.Energy;
                                        entities[i].RewardAction(0.01f);
                                    }
                                }
                                else
                                {
                                    var dist = (ent.Location.X - entities[i].Location.X) * (ent.Location.X - entities[i].Location.X) +
                                        (ent.Location.Y - entities[i].Location.Y) * (ent.Location.Y - entities[i].Location.Y);
                                    if (dist < 10)
                                    {
                                        ent.BurnEnergy(5);
                                        entities[i].BurnEnergy(5);
                                        entities[i].MarkForRemoval = true;

                                        ent.Energy += entities[i].Energy;
                                        ent.RewardAction(0.01f);
                                    }
                                }
                            }
                        }
                    }
                    ent.ExtFlag = 0;
                }

                var dyingEnts = entities.Where(x => x.Energy < 40).ToList();
                foreach (SimEntity ent in entities)
                {
                    for(int i = 0;i<dyingEnts.Count;i++)
                    {
                        var dist = (ent.Location.X - entities[i].Location.X) * (ent.Location.X - entities[i].Location.X) +
                                        (ent.Location.Y - entities[i].Location.Y) * (ent.Location.Y - entities[i].Location.Y);
                        if (dist < 20)
                        {
                            if (entities[i].GetSimiliarity(ent.actionVector) <= ent.actionVector[1])
                            {
                                ent.Energy += (int)(entities[i].Energy / 4.0f);
                                entities[i].Energy = (int)(entities[i].Energy * (3 / 4.0f));

                                List<float> oldSensorVector = new List<float>();
                                oldSensorVector = entities[i].sensorVector;

                                //interaction
                                for (int j=0;j<entities[i].sensorVector.Count;j++)
                                {
                                    entities[i].sensorVector[j] = entities[i].sensorVector[j] + ent.sensorVector[j] * entities[i].actionVector[1];
                                    if(entities[i].sensorVector[j] == 1.0f)
                                    {
                                        entities[i].sensorVector[j] = 1.0f;
                                    }
                                    else if(entities[i].sensorVector[j] == -1.0f)
                                    {
                                        entities[i].sensorVector[j] = -1.0f;
                                    }
                                }
                                for (int j = 0; j < ent.sensorVector.Count; j++)
                                {
                                    ent.sensorVector[j] = ent.sensorVector[j] + oldSensorVector[j] * ent.actionVector[1];
                                    if (ent.sensorVector[j] == 1.0f)
                                    {
                                        ent.sensorVector[j] = 1.0f;
                                    }
                                    else if (ent.sensorVector[j] == -1.0f)
                                    {
                                        ent.sensorVector[j] = -1.0f;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach(SimEntity ent in entities.Where(x => x.MarkForRemoval).ToList())
                {
                    for(int i=0;i<entities.Count;i++)
                    {
                        var dist = (ent.Location.X - entities[i].Location.X) * (ent.Location.X - entities[i].Location.X) +
                                        (ent.Location.Y - entities[i].Location.Y) * (ent.Location.Y - entities[i].Location.Y);
                        if(dist < 20 && ent.Age < 40 && ent.Energy < 10)
                        {
                            for(int j=0;j<entities[i].sensorVector.Count;j++)
                            {
                                entities[i].sensorVector[j] = entities[i].actionVector[0] - entities[i].sensorVector[j];
                            }
                        }
                    }
                }

                entities.RemoveAll(x => x.MarkForRemoval == true);
            }

            picViewport.Refresh();
        }
    }
}
