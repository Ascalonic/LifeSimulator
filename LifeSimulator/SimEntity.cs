using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeSimulator
{
    public class SimEntity
    {
        public Point Location { get; set; }
        public int Size { get; set; }
        public int Age { get; set; }
        public int MaxAge { get; set; }
        public bool MarkForRemoval { get; set; }
        public int Energy { get; set; }

        public List<float> Inputs { get; set; }
        public int ExtFlag = 0;
        public List<float> actionVector { get; set; }
        public List<float> sensorVector { get; set; }
        public int Generation { get; set; }

        public List<float> avHistory { get; set; }

        private int state = 0;

        public SimEntity(Random random)
        {
            Age = 0;
            if (random != null)
            {
                actionVector = new List<float>() {
                    random.Next(-10, 10)/(11.0f),   // direction decision bias
                    random.Next(-10, 10) / (11.0f), // kill kins
                    random.Next(-10, 10) / (11.0f)  // inheritance threshold
                };

                sensorVector = new List<float>() {
                    random.Next(-10, 10) / (11.0f),
                    random.Next(-10, 10) / (11.0f),
                    random.Next(-10, 10) / (11.0f),
                    random.Next(-10, 10) / (11.0f)
                };

                avHistory = new List<float>();
                avHistory = actionVector;
            }
            state = 1;
            Inputs = new List<float>();
        }

        public SimEntity(List<float> newVector)
        {
            Age = 0;
            actionVector = newVector;
            state = 1;
            Inputs = new List<float>();
            Generation = 1;
        }

        public void Draw(Graphics g)
        {
            Color fillColor;
            if (ExtFlag == 2)
            {
                fillColor = Color.FromArgb(MaxAge - Age, 0, 0);
            }
            else
            {
                fillColor = Color.FromArgb(0, 0, MaxAge - Age);
            }

            g.FillRectangle(new SolidBrush(fillColor), new Rectangle(Location.X, Location.Y, Size, Size));
        }

        public double GetSimiliarity(List<float> actionVector2)
        {
            return Math.Sqrt((actionVector[0] - actionVector2[0]) * (actionVector[0] - actionVector2[0]) +
                (actionVector[1] - actionVector2[1]) * (actionVector[1] - actionVector2[1]) +
                (actionVector[2] - actionVector2[2]) * (actionVector[2] - actionVector2[2]));
        }

        public void IncrementAge()
        {
            Age++;
            if (Age >= MaxAge)
            {
                MarkForRemoval = true;
            }
        }

        private Point SelectionMovementDirection()
        {
            float sum = 0.0f;
            for(int i=0;i<Inputs.Count;i++)
            {
                sum += Inputs[i] * sensorVector[i];
            }
            if(sum >= 0 && sum <= Math.Abs(actionVector[0]/4))
            {
                return new Point(2, 2);
            }
            else if(sum >= Math.Abs(actionVector[0] / 4) && sum <= Math.Abs(actionVector[0] / 2))
            {
                return new Point(2, -2);
            }
            else if (sum >= Math.Abs(actionVector[0] / 2) && sum <= Math.Abs(actionVector[0]* 3 / 4))
            {
                return new Point(-2, -2);
            }
            else if (sum >= Math.Abs(actionVector[0]* 3 / 4))
            {
                return new Point(-2, 2);
            }
            else
            {
                return new Point(0, 0);
            }
        }

        private void Move(Point moveVector)
        {
            if(Math.Abs(moveVector.X) >= 0 || Math.Abs(moveVector.Y) >= 0)
            {
                Location = new Point(Location.X + moveVector.X, Location.Y + moveVector.Y);
                BurnEnergy(Size/2);
            }
        }

        public void BurnEnergy(int amt)
        {
            Energy -= amt;
            if(Energy <= 0)
            {
                MarkForRemoval = true;
            }
        }

        private void NextState()
        {
            float nextState = 0.0f;
            for(int i=0;i<actionVector.Count;i++)
            {
                nextState += (state * actionVector[i]);
            }
            state = (Math.Abs((int)(nextState * (Age + 10)))) % 3 + 1;
        }

        public SimEntity CreateNewEntity(Random random)
        {
            var v1 = random.Next(-10, 10) / (11.0f);
            var value = (random.Next(-10, 10) / (11.0f) + actionVector[0]) / 2;

            List<float> newVector = new List<float>();
            newVector = new List<float>() { (random.Next(-10, 10)/(11.0f) + actionVector[0])/2,
                (random.Next(-10, 10)/(11.0f) + actionVector[1]) / 2, (random.Next(-10, 10)/(11.0f) + actionVector[2]) / 2 };

            SimEntity newEntity = new SimEntity(newVector)
            {
                Location = new Point(Location.X + random.Next(-10, 10), Location.Y + random.Next(-10, 10)),
                MaxAge = MaxAge,
                Size = Size + (int)(actionVector[actionVector.Count - 1] * 2),
                Energy = (int)(Energy * actionVector[actionVector.Count - 1]),
                Generation = Generation + 1,
                avHistory = avHistory,
                sensorVector = sensorVector
            };

            BurnEnergy(10);
            return newEntity;
        }

        public void RewardAction(float rewardQuotient)
        {
            for(int i=0;i<actionVector[i];i++)
            {
                actionVector[i] = actionVector[i] + actionVector[i] * rewardQuotient;
                if (actionVector[i] > 1.0f)
                    actionVector[i] = 1.0f;
                else if (actionVector[i] < -1.0f)
                    actionVector[i] = -1.0f;
            }
            if(state == 1)
            {
                for (int i = 0; i < sensorVector.Count; i++)
                {
                    sensorVector[i] = sensorVector[i] + sensorVector[i] * rewardQuotient;
                    if (sensorVector[i] > 1.0f)
                        sensorVector[i] = 1.0f;
                    else if (sensorVector[i] < -1.0f)
                        sensorVector[i] = -1.0f;
                }
            }
        }

        public void NextAction()
        {
            BurnEnergy(1);
            NextState();

            if (state == 1)
            {
                Move(SelectionMovementDirection());
            }
            else if(state == 2)
            {
                if(Age >= MaxAge/2.0 - 40 && Age <= MaxAge / 2.0 + 40)
                {
                    ExtFlag = 1;
                }
            }
            else if(state == 3)
            {
                //predator
                ExtFlag = 2;
            }
        }
    }
}
