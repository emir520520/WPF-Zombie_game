using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Midterm_zombie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Random r = new Random();

        public static string name;
        int row;
        int col;
        int humanNumber;
        int zombieNumber;
        string option;
        int iterationNumber=-1;
        int iter = 0;

        //Humans and Zombies
        List<Human> humans = new List<Human>();
        List<Zombie> zombies = new List<Zombie>();
        List<Human> BackUphumans = new List<Human>();

        //For randomly generate location
        List<string> occupied = new List<string>();

        //StreamWriter for Movements
        public static string fileName;
        System.IO.StreamWriter streamWriterMovements;

        //List of infections
        List<Infection> infections = new List<Infection>();
        List<Infection> BackUpinfections = new List<Infection>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseOption(object sender, RoutedEventArgs e)
        {
            option = (string)(sender as RadioButton).Content;

            if (String.Equals(option, "Wait until all become zombies"))
            {
                Spe.IsReadOnly = true;
                Spe.Background = Brushes.Gray;
                Spe.Text = "";
            }
            else
            {
                Spe.IsReadOnly = false;
                Spe.Background = Brushes.White;
            }
        }

        private async void Simulation(object sender, RoutedEventArgs e)
        {
            try
            {
                name = Name.Text;
                row = Convert.ToInt32(Row.Text);
                col = Convert.ToInt32(Col.Text);
                humanNumber = Convert.ToInt32(Human.Text);
                zombieNumber = Convert.ToInt32(Zombie.Text);

                fileName = "C:\\" + name + "_Movements.txt";

                if (option == "Specify")
                {
                    iterationNumber = Convert.ToInt32(Spe.Text);

                    if (iterationNumber <= 0)
                    {
                        throw new Exception("Please enter a positive number for iteration times");
                    }
                }
                if (name == "")
                {
                    throw new Exception("Please enter your name");
                }
                if (option == null)
                {
                    throw new Exception("Please choose one option");
                }
                if (row < 0 || col < 0)
                {
                    throw new Exception("Please enter positive values");
                }
                if(humanNumber<5)
                {
                    throw new Exception("Please enter at least 5 humans");
                }
                if (zombieNumber < 1)
                {
                    throw new Exception("Please enter at least 1 zombie");
                }

                //Clear row and column definition and all the childs
                MyGrid.Children.Clear();
                MyGrid.RowDefinitions.Clear();
                MyGrid.ColumnDefinitions.Clear();

                //Generate table
                GenerateTable(row, col);

                //Clear the humans and zombies lists
                humans.Clear();
                zombies.Clear();

                //Generate random location for each human and zombie
                init('H');
                init('Z');
                await Task.Delay(1233);

                //Iteration
                streamWriterMovements = new System.IO.StreamWriter(fileName, false);
                if (iterationNumber == -1)
                {
                    while (humans.Count > 0)
                    {
                        Ite();
                        await Task.Delay(1233);
                    }

                    //Display the result 
                    MessageBox.Show("It took " + iter + " iterations for all the humans became zombies", "Result", MessageBoxButton.OK, MessageBoxImage.Information);

                    //Write zombie infecting file
                    string fileName = "C:\\" + name + "_DoingTheinfecting.txt";
                    System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(fileName, false);

                    for(int i = 0; i < zombies.Count; i++)
                    {
                        for(int k=0;k< BackUpinfections.Count; k++)
                        {
                            if (BackUpinfections[k].infectorNumber == (i + 1))
                            {
                                streamWriter.Write("Zombie " + (i + 1) + ": Infected Human " + BackUpinfections[k].infectedIndex + " at Iteration " + (BackUpinfections[k].ite+1) + "  ");
                            }
                        }
                        streamWriter.WriteLine("");
                    }
                    streamWriter.Close();

                    //Write human infected file
                    fileName = "C:\\" + name + "_InfectedBy.txt";
                    streamWriter = new System.IO.StreamWriter(fileName, false);

                    for (int i = 0; i < BackUphumans.Count; i++)
                    {
                        for (int k = 0; k < BackUpinfections.Count; k++)
                        {
                            if (BackUpinfections[k].infectedIndex == (i + 1))
                            {
                                streamWriter.Write("Human " + (i + 1) + " Infected by: Zombie " + BackUpinfections[k].infectorNumber + " at Iteration " + (BackUpinfections[k].ite + 1) + "  ");
                            }
                        }
                        streamWriter.WriteLine("");
                    }
                    streamWriter.Close();

                    //Write the final result
                    fileName = "C:\\" + name + "_FinalResults.txt";
                    streamWriter = new System.IO.StreamWriter(fileName, false);

                    streamWriter.WriteLine("The number of humans: " + humans.Count + "\n" + "The number of zombies: " + zombies.Count + "\n" + "The survival rate of humans: " + ((float)humans.Count / humanNumber * 100) + "%\n"+"The number of iterations: "+(iter+1));
                    streamWriter.Close();
                }
                else
                {
                    while (iter < iterationNumber && humans.Count>0)
                    {
                        Ite();
                        await Task.Delay(1233);
                    }

                    //Display the result
                    if (humans.Count == 0)
                    {
                        MessageBox.Show("It took " + iter + " iterations for all the humans became zombies", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
                    }else if(iter== iterationNumber)
                    {
                        MessageBox.Show("The number of humans: "+humans.Count+"\n"+"The number of zombies: "+zombies.Count+"\n"+"The survival rate of humans: "+((float) humans.Count/humanNumber*100)+"%", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    //Write zombie infecting file
                    string fileName = "C:\\" + name + "_DoingTheinfecting.txt";
                    System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(fileName, false);

                    for (int i = 0; i < zombies.Count; i++)
                    {
                        for (int k = 0; k < BackUpinfections.Count; k++)
                        {
                            if (BackUpinfections[k].infectorNumber == (i + 1))
                            {
                                streamWriter.Write("Zombie " + (i + 1) + ": Infected Human " + BackUpinfections[k].infectedIndex + " at Iteration " + (BackUpinfections[k].ite+1) + "  ");
                            }
                        }
                        streamWriter.WriteLine("");
                    }
                    streamWriter.Close();

                    //Write human infected file
                    fileName = "C:\\" + name + "_InfectedBy.txt";
                    streamWriter = new System.IO.StreamWriter(fileName, false);

                    for (int i = 0; i < BackUphumans.Count; i++)
                    {
                        for (int k = 0; k < BackUpinfections.Count; k++)
                        {
                            if (BackUpinfections[k].infectedIndex == (i + 1))
                            {
                                streamWriter.Write("Human " + (i + 1) + " Infected by: Zombie " + BackUpinfections[k].infectorNumber + " at Iteration " + (BackUpinfections[k].ite + 1) + "  ");
                            }
                        }
                        streamWriter.WriteLine("");
                    }
                    streamWriter.Close();

                    //Write the final result
                    fileName = "C:\\" + name + "_FinalResults.txt";
                    streamWriter = new System.IO.StreamWriter(fileName, false);

                    streamWriter.WriteLine("The number of humans: " + humans.Count + "\n" + "The number of zombies: " + zombies.Count + "\n" + "The survival rate of humans: " + ((float)humans.Count / humanNumber * 100) + "%\n" + "The number of iterations: " + (iter + 1));
                    streamWriter.Close();
                }
                streamWriterMovements.Close();

                //Reset
                iter = 0;
                occupied.Clear();
                iterationNumber = -1;
                humans.Clear();
                zombies.Clear();
                infections.Clear();
                BackUpinfections.Clear();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            } 
        }

        public void GenerateTable(int row, int col)
        {
            for (int x = 0; x < row; x++)
                MyGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            for (int y = 0; y < col; y++)
            {
                MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    TextBox tb = new TextBox();
                    tb.Height = 520 / row;
                    Grid.SetRow(tb, x);
                    Grid.SetColumn(tb, y);
                    MyGrid.Children.Add(tb);
                }
            }
        }

        //Generate random geolocation for human and zombie and show
        public void init(char option)
        {
            int geoX;
            int geoY;

            //Currently randomly generated geolocation
            string generated;

            //For element in occupied list
            string occu;

            if (option == 'H')
            {
                for (int i = 0; i < humanNumber; i++)
                {
                    Human hu = new Human();

                    Here1:
                    //geoX (row)
                    geoX = r.Next(0, row);

                    //geoY (col)
                    geoY = r.Next(0, col);

                    //Actual order of generated location
                    generated = geoX + " "+geoY;

                    //Validation
                    for (int k = 0; k < occupied.Count; k++)
                    {
                        if(occupied[k] == generated)
                        {
                            goto Here1;
                        }
                    }
                    occupied.Add(generated);

                    ((TextBox)MyGrid.Children[(geoX * col) + geoY]).Text = "H" + (i + 1)+" ";
                    hu.name = "H" + (i+1);
                    hu.geoX = geoX;
                    hu.geoY = geoY;
                    humans.Add(hu);
                    BackUphumans.Add(hu);
                }
            }
            else if (option == 'Z')
            {
                for (int i = 0; i < zombieNumber; i++)
                {
                    Zombie zo = new Zombie();

                    Here2:
                    //geoX (row)
                    geoX = r.Next(0, row);

                    //geoY (col)
                    geoY = r.Next(0, col);

                    //Actual order of generated location
                    generated =geoX + " " + geoY;

                    //Validation
                    for (int k = 0; k < occupied.Count; k++)
                    {
                        if (occupied[k] == generated)
                        {
                            goto Here2;
                        }
                    }
                    occupied.Add(generated);

                    ((TextBox)MyGrid.Children[(geoX * col) + geoY]).Text = "Z" + (i + 1)+" ";
                    zo.name = "Z" + (i + 1);
                    zo.geoX = geoX;
                    zo.geoY = geoY;
                    zombies.Add(zo);
                }
            }

            //Write starting configuration into file
            string fileName = "C:\\" + name + "_StartingConfiguration.txt";
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(fileName,false);

            for(int i = 0; i < humans.Count; i++)
            {
                streamWriter.WriteLine("Human "+(i+1)+" starts at "+humans[i].geoX+","+humans[i].geoY);
            }
            streamWriter.WriteLine(" ");

            for (int i = 0; i < zombies.Count; i++)
            {
                streamWriter.WriteLine("Zombie " + (i + 1) + " starts at " + zombies[i].geoX + "," + zombies[i].geoY);
            }

            streamWriter.Close();
        }

        public void Ite()
        {
            int geoX;
            int geoY;
            int X;
            int Y;
            int posi;

            //For availabel block to move to for each human and zombie
            List<int> avai = new List<int>();

            streamWriterMovements.WriteLine("Iteration {0}", iter+1);

            //Get the location of each human and zombie and move randomly
            for (int i = 0; i < humans.Count; i++)
            {
                geoX = humans[i].geoX;
                geoY = humans[i].geoY;

                //Check Availability for the position it can move to
                //top left
                X = ((((geoX * col) + geoY) - (col+1)) / col);
                Y = ((((geoX * col) + geoY) - (col + 1)) % col);

                if ((X==geoX-1) && (Y == geoY - 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y)>=0)
                {
                    avai.Add((X * col) + Y);
                }

                //top middle
                X = ((((geoX * col) + geoY) - col) / col);
                Y = ((((geoX * col) + geoY) - col) % col);

                if ((X == geoX - 1) && (Y == geoY) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //top right
                X = ((((geoX * col) + geoY) - (col - 1)) / col);
                Y = ((((geoX * col) + geoY) - (col - 1)) % col);

                if ((X == geoX - 1) && (Y == geoY + 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //left
                X = ((((geoX * col) + geoY) - 1) / col);
                Y = ((((geoX * col) + geoY) - 1) % col);

                if ((X == geoX) && (Y == geoY - 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //right
                X = ((((geoX * col) + geoY) +1) / col);
                Y = ((((geoX * col) + geoY) +1) % col);

                if ((X == geoX) && (Y == geoY + 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //bottom left
                X = ((((geoX * col) + geoY) + (col - 1)) / col);
                Y = ((((geoX * col) + geoY) + (col - 1)) % col);

                if ((X == geoX + 1) && (Y == geoY - 1) && ((X * col) + Y)<=(col*row-1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //bottom middle
                X = ((((geoX * col) + geoY) + col) / col);
                Y = ((((geoX * col) + geoY) +col) % col);

                if ((X == geoX +1) && (Y == geoY) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //bottom right
                X = ((((geoX * col) + geoY) + (col + 1)) / col);
                Y = ((((geoX * col) + geoY) + (col + 1)) % col);

                if ((X == geoX + 1) && (Y == geoY + 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //Get random position
                posi=avai[r.Next(0, avai.Count)];
                X = posi / col;
                Y = posi % col;

                //Write into File (Movement)
                streamWriterMovements.WriteLine("From " + humans[i].geoX + "," + humans[i].geoY + " Human " + humans[i].name.Substring(1) + " is now at " + X + "," + Y);

                //Remove from the orginal position
                int location=geoX*col+geoY;
                ((TextBox)MyGrid.Children[location]).Text=((TextBox)MyGrid.Children[location]).Text.Replace(humans[i].name + " ", "");

                //Substitude
                humans[i].geoX = X;
                humans[i].geoY = Y;

                //Show the move
                ((TextBox)MyGrid.Children[(X * col) + Y]).Text +=humans[i].name+" ";

                //Clear the avai list
                avai.Clear();
            }

            streamWriterMovements.WriteLine();

            for (int i = 0; i < zombies.Count; i++)
            {
                geoX = zombies[i].geoX;
                geoY = zombies[i].geoY;

                //Check Availability for the position it can move to
                //top left
                X = ((((geoX * col) + geoY) - (col + 1)) / col);
                Y = ((((geoX * col) + geoY) - (col + 1)) % col);

                if ((X == geoX - 1) && (Y == geoY - 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //top middle
                X = ((((geoX * col) + geoY) - col) / col);
                Y = ((((geoX * col) + geoY) - col) % col);

                if ((X == geoX - 1) && (Y == geoY) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //top right
                X = ((((geoX * col) + geoY) - (col - 1)) / col);
                Y = ((((geoX * col) + geoY) - (col - 1)) % col);

                if ((X == geoX - 1) && (Y == geoY + 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //left
                X = ((((geoX * col) + geoY) - 1) / col);
                Y = ((((geoX * col) + geoY) - 1) % col);

                if ((X == geoX) && (Y == geoY - 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //right
                X = ((((geoX * col) + geoY) + 1) / col);
                Y = ((((geoX * col) + geoY) + 1) % col);

                if ((X == geoX) && (Y == geoY + 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //bottom left
                X = ((((geoX * col) + geoY) + (col -1)) / col);
                Y = ((((geoX * col) + geoY) + (col-1)) % col);

                if ((X == geoX + 1) && (Y == geoY - 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //bottom middle
                X = ((((geoX * col) + geoY) + col) / col);
                Y = ((((geoX * col) + geoY) + col) % col);

                if ((X == geoX + 1) && (Y == geoY) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //bottom right
                X = ((((geoX * col) + geoY) + (col + 1)) / col);
                Y = ((((geoX * col) + geoY) + (col+1)) % col);

                if ((X == geoX + 1) && (Y == geoY + 1) && ((X * col) + Y) <=(col * row - 1) && ((X * col) + Y) >=0)
                {
                    avai.Add((X * col) + Y);
                }

                //Get random position
                posi = avai[r.Next(0, avai.Count)];
                X = posi / col;
                Y = posi % col;

                //Write into File (Movement)
                streamWriterMovements.WriteLine("From " + zombies[i].geoX + "," + zombies[i].geoY + " Zombie " + zombies[i].name.Substring(1) + " is now at " + X + "," + Y);

                //Remove from the orginal position
                int location = geoX * col + geoY;
                ((TextBox)MyGrid.Children[location]).Text=((TextBox)MyGrid.Children[location]).Text.Replace(zombies[i].name + " ", "");

                //Substitude
                zombies[i].geoX = X;
                zombies[i].geoY = Y;

                //Show the move
                ((TextBox)MyGrid.Children[(X * col) + Y]).Text += zombies[i].name + " ";

                //Clear the avai list
                avai.Clear();
            }
            streamWriterMovements.WriteLine();
            streamWriterMovements.WriteLine("-----------------------------------");

            //Check infection
            List<int> backInfectors;
            for(int i = 0; i < humans.Count; i++)
            {
                backInfectors = new List<int>();
                for (int k = 0; k < zombies.Count; k++)
                {
                    if ((humans[i].geoX == zombies[k].geoX) && (humans[i].geoY == zombies[k].geoY)){
                        backInfectors.Add(k);
                    }
                }

                if (backInfectors.Count > 0)
                {
                    //Create a new infection instance and store it in infections list
                    Infection infection = new Infection();
                    infection.infectorNumber = backInfectors[r.Next(0, backInfectors.Count)]+1;
                    infection.infectedName = humans[i].name;
                    infection.infectedIndex = Int32.Parse(humans[i].name.Substring(1));
                    infection.infectedX= humans[i].geoX;
                    infection.infectedY = humans[i].geoY;
                    infection.ite = iter;
                    infections.Add(infection);
                    BackUpinfections.Add(infection);
                }
            }

            for(int i = infections.Count-1; i >=0; i--)
            {
                //Add the new infected human to zombies list
                Zombie zombie = new Zombie();
                zombie.name = "Z" + (zombies.Count + 1);
                zombie.geoX = infections[i].infectedX;
                zombie.geoY = infections[i].infectedY;
                zombies.Add(zombie);

                //Remove the human from humans list and remove it from the geolocation
                int location = infections[i].infectedX * col + infections[i].infectedY;
                var itemToRemove = humans.Single(hu => hu.name == infections[i].infectedName);
                humans.Remove(itemToRemove);
                if (humans.Count>0)
                {
                    ((TextBox)MyGrid.Children[location]).Text = ((TextBox)MyGrid.Children[location]).Text.Replace(infections[i].infectedName + " ", "");
                }
            }
            infections.Clear();
            iter++;
        }
    }
}