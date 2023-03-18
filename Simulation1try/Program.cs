using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation1try
{
    internal class SimulationMain
    {
        static int generation = 0;
        //static int fps = 20;
        static int actual_fps = 0;
        static int frames = 0;
        static bool frame_by_frame = false;
        public static int height = 30;
        public static int width = 50;
        public static int[][][] field = new int[height][][];
        public static List<Cell> cells = new List<Cell>();
        public static List<Cell> buff = new List<Cell>();
        public static List<Cell> to_remove = new List<Cell>();
        static Random random = new Random();
        static string path_folder = @"C:\Users\lucky\Documents\DataSimulation\";
        static void Main(string[] args)
        {
            Thread fps_thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    actual_fps = frames;
                    frames = 0;
                }
            });
            fps_thread.Start();
            Console.ReadLine();
            while (true)
            {
                
                generation++;


                string gen_file_path = path_folder + "gen_" + generation + ".txt";
                File.Create(gen_file_path).Close();
                FileStream fs = new FileStream(gen_file_path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter writer = new StreamWriter(fs);


                field = new int[height][][];
                cells = new List<Cell>();
                buff = new List<Cell>();
                to_remove = new List<Cell>();
                FieldArrayInit();

                cells.Add(new Cell(7, 8, random.Next(), 0));

                cells.Add(new Cell(27, 28, random.Next(), 0));



                while (true)
                {
                    writer.WriteLine(cells.Count());
                    RefreshArrayCellPosition();
                    DisplayField(0);

                    foreach (Cell cell in cells)
                    {
                        cell.NextMove();
                        if (cell.health <= 0)
                        {
                            to_remove.Add(cell);
                        }
                    }
                    foreach (Cell cell in buff)
                    {
                        cells.Add(cell);
                    }
                    foreach (Cell cell in to_remove)
                    {
                        cells.Remove(cell);
                    }
                    buff.Clear();
                    to_remove.Clear();
                    if (frame_by_frame) { Console.ReadLine(); }
                    Thread.Sleep(1);
                    if (cells.Count == 0)
                    {
                        break;
                    }
                    frames++;
                }
                writer.Close();
            }
            Console.ReadLine();
        }

        static void FieldArrayInit() 
        {
            for (int i = 0; i < height; i++)
            {
                field[i] = new int[width][];
                for (int j = 0; j < width; j++)
                {
                    field[i][j] = new int[2];
                    field[i][j][0] = 0;
                    field[i][j][1] = 99;
                }
            }
        }
        static void DisplayField(int mode)
        {
            Console.Clear();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if(field[i][j][mode] == 0)
                    {
                        Console.Write(" |");
                    }
                    else
                    {
                        Console.Write("W|");
                    }
                    
                }
                Console.WriteLine();
            }
            Console.WriteLine("\nPopulation: " + cells.Count);
            Console.WriteLine("Generation: " + generation);
            Console.WriteLine("FPS: " + actual_fps);
        }
        static void RefreshArrayCellPosition()
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    field[i][j][0] = 0;
                }
            }
            foreach (Cell cell in cells)
            {
                field[cell.y][cell.x][0] = 1;
            }
        }
    }

    class Cell
    {
        public int x;
        public int y;

        public double speed;
        public double defence = 10;
        public double attack = 20;

        public double health = 50;
        public double energy = 0;

        public static int genom_length = 128;
        public static int genom_options = 3;
        public static int genom_move_options = 16;

        public int[][] genom = new int[genom_length][];

        public int active_gen = 0;
        Random rn;

        public Cell(int x_, int y_, int seed, int active_gen_)
        {
            x = x_;
            y = y_;
            active_gen = active_gen_;
            rn = new Random(seed);

            for (int i = 0; i < genom_length; i++)
            {
                genom[i] = new int[genom_options];
                genom[i][0] = rn.Next() % genom_length;
                genom[i][1] = rn.Next() % genom_move_options;
                genom[i][2] = rn.Next() % genom_length;
            }
        }

        public Cell(int x_, int y_, int seed, int active_gen_, int[][] genom_, double defence_, double attack_)
        {
            x = x_;
            y = y_;
            defence = defence_;
            attack = attack_;
            active_gen = active_gen_;
            rn = new Random(seed);
            bool is_mutating = false;
            int m_x = 0;
            int m_y = 0;
            int m_v = 0;
            if(rn.Next() %100 < 10)
            {
                is_mutating = true;
                m_x = rn.Next()%genom_length;
                m_y = rn.Next()%genom_options;
                switch (m_y)
                {
                    case 0:
                        m_v = rn.Next() % genom_length;
                        break;
                    case 1:
                        m_v = rn.Next() % genom_move_options;
                        break;
                    case 2:
                        m_v = rn.Next() % genom_length;
                        break;
                }
                
            }
            int j = rn.Next();
            if (j % 1000 < 100)
            {
                if (j % 2 == 0) 
                {
                    defence += rn.Next() % 11 - 20;
                    if(defence < 1)
                    {
                        defence = 1;
                    }
                }
                else
                {
                    attack += rn.Next() % 11 - 20;
                    if (attack < 1)
                    {
                        attack = 1;
                    }
                }
            }

            for (int i = 0; i < genom_length; i++)
            {
                genom[i] = new int[genom_options];
                for(int k = 0; k < genom_options; k++)
                {
                    genom[i][k] = genom_[i][k];
                }
            }
            if (is_mutating)
            {
                genom[m_x][m_y] = m_v;
            }
        }

        public void NextMove()
        {
            int count_how_many = 0;
            if(CheckPosition(x+1, y, true))
            {
                count_how_many++;
            }
            if (CheckPosition(x - 1, y, true))
            {
                count_how_many++;
            }
            if (CheckPosition(x, y+1, true))
            {
                count_how_many++;
            }
            if (CheckPosition(x, y-1, true))
            {
                count_how_many++;
            }


            if (count_how_many == 0)
            {
                count_how_many = 1;
            }
            health -=count_how_many;
            
            energy+= 1/count_how_many;
            int next_gen = genom[active_gen][0];
            int move_type = genom[active_gen][1];
            int breed_number = genom[active_gen][2];
            active_gen = next_gen;

            if(move_type == 0)
            {
                energy += 3/count_how_many;
            }else
            if(move_type >= 1 && move_type <= 4 && energy >= 5)
            {
                if (Move(move_type))
                {
                    energy -= 5;
                };
            } else
            if(move_type >= 5 && move_type <= 8 && energy >= 30)
            {
                
                if(Breed(breed_number, move_type - 4))
                {
                    energy -= 30;
                };
            }else
            if(move_type >= 9 && move_type <= 12 && energy >= 7)
            {
                int act_att = Attack(move_type-8);
                energy += act_att * 10;
                if (act_att != 0)
                {
                    energy -= 7;
                }
            }
            else
            {
                energy += 1 / count_how_many;
            }
        }
        public int Attack(int direction)
        {
            switch (direction)
            {
                case 1:
                    if (CheckPosition(x + 1, y, true))
                    {
                        return GetCellFromCoord(x + 1, y).GetDamage(attack);
                    }
                    break;
                case 2:
                    if (CheckPosition(x, y+1, true))
                    {
                        return GetCellFromCoord(x, y+1).GetDamage(attack);
                    }
                    break;
                case 3:
                    if (CheckPosition(x - 1, y, true))
                    {
                        return GetCellFromCoord(x - 1, y).GetDamage(attack);
                    }
                    break;
                case 4:
                    if (CheckPosition(x, y-1, true))
                    {
                        return GetCellFromCoord(x, y-1).GetDamage(attack);
                    }
                    break;
            }
            return 0;
        }
        public int GetDamage(double dmg)
        {
            double actual_dmg = (dmg * 20) / defence;
            health -= actual_dmg;
            return (int)actual_dmg;
        }
        public Cell GetCellFromCoord(int x_w, int y_w)
        {
            foreach (Cell cell in SimulationMain.cells)
            {
                if(cell.x == x_w && cell.y == y_w)
                {
                    return cell;
                }
            }
            return null;
        }
        public bool Breed(int gen, int direction)
        {
            switch (direction)
            {
                case 1:
                    if (CheckPosition(x + 1, y, false))
                    {
                        SimulationMain.buff.Add(new Cell(x + 1, y, rn.Next(), gen, genom, defence, attack));
                    } else { return false; }
                    break;
                case 2:
                    if (CheckPosition(x, y+1, false))
                    {
                        SimulationMain.buff.Add(new Cell(x, y+1, rn.Next(), gen, genom, defence, attack));
                    }
                    else { return false; }
                    break;
                case 3:
                    if (CheckPosition(x - 1, y, false))
                    {
                        SimulationMain.buff.Add(new Cell(x - 1, y, rn.Next(), gen, genom, defence, attack));
                    }
                    else { return false; }
                    break;
                case 4:
                    if (CheckPosition(x, y-1, false))
                    {
                        SimulationMain.buff.Add(new Cell(x, y-1, rn.Next(), gen, genom, defence, attack));
                    }
                    else { return false; }
                    break;
            }
            return true;
        }

        public bool Move(int direction)
        {
            int x_w = x;
            int y_w = y;
            switch (direction)
            {
                case 1:
                    x_w++;
                    break;
                case 2:
                    y_w--;
                    break;
                case 3:
                    x_w--;
                    break;
                case 4:
                    y_w++;
                    break;
            }
            if (CheckPosition(x_w, y_w, false))
            {
                x = x_w;
                y = y_w;
            }
            else { return false; }
            return true;
        }
        bool CheckPosition(int w_x, int w_y, bool check_occup)
        {
            if (w_x < SimulationMain.width && w_y < SimulationMain.height && w_x >= 0 && w_y >= 0)
            {
                bool is_occupied = false;
                foreach(Cell cell in SimulationMain.cells)
                {
                    if(cell.x == w_x && cell.y == w_y)
                    {
                        is_occupied = true;
                    }
                }
                if (!check_occup)
                {
                    foreach (Cell cell in SimulationMain.buff)
                    {
                        if (cell.x == w_x && cell.y == w_y)
                        {
                            is_occupied = true;
                        }
                    }
                }
                if (check_occup)
                {
                    return is_occupied;
                }
                return !is_occupied;
            }
            return false;
        }
    }
}
