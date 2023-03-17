using System;
using System.Collections.Generic;
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
        static int fps = 100;
        static bool frame_by_frame = false;
        public static int height = 30;
        public static int width = 50;
        public static int[][][] field = new int[height][][];
        public static List<Cell> cells = new List<Cell>();
        public static List<Cell> buff = new List<Cell>();
        public static List<Cell> to_remove = new List<Cell>();
        static Random random = new Random();
        static void Main(string[] args)
        {
            Console.ReadLine();
            while (true)
            {
                generation++;
                field = new int[height][][];
                cells = new List<Cell>();
                buff = new List<Cell>();
                to_remove = new List<Cell>();
                FieldArrayInit();

                cells.Add(new Cell(7, 8, random.Next(), 0));

                cells.Add(new Cell(27, 28, random.Next(), 0));



                while (true)
                {
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
                    Thread.Sleep(1000 / fps);
                    if (cells.Count == 0)
                    {
                        break;
                    }
                }
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

        public int[][] genom = new int[16][];

        public int active_gen = 0;
        Random rn;

        public Cell(int x_, int y_, int seed, int active_gen_)
        {
            x = x_;
            y = y_;
            active_gen = active_gen_;
            rn = new Random(seed);

            for (int i = 0; i < 16; i++)
            {
                genom[i] = new int[3];
                genom[i][0] = rn.Next() % 16;
                genom[i][1] = rn.Next() % 16;
                genom[i][2] = rn.Next() % 16;
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
                m_x = rn.Next()%16;
                m_y = rn.Next()%3;
                m_v = rn.Next()%16;
            }
            int j = rn.Next();
            if (j % 1000 < 50)
            {
                if (j % 2 == 0) 
                {
                    defence += rn.Next()%11-5;
                }
                else
                {
                    attack += rn.Next() % 11 - 5;
                }
            }

            for (int i = 0; i < 16; i++)
            {
                genom[i] = new int[3];
                genom[i][0] = genom_[i][0];
                genom[i][1] = genom_[i][0];
                genom[i][2] = genom_[i][0];
            }
            if (is_mutating)
            {
                genom[m_x][m_y] = m_v;
            }
        }

        public void NextMove()
        {
            health--;
            energy+= 1;
            int next_gen = genom[active_gen][0];
            int move_type = genom[active_gen][1];
            int breed_number = genom[active_gen][2];
            active_gen = next_gen;

            if(move_type == 0)
            {
                energy += 4;
            }else
            if(move_type >= 1 && move_type <= 4 && energy >= 5)
            {
                energy -= 5;
                Move(move_type);
            } else
            if(move_type >= 5 && move_type <= 8 && energy >= 30)
            {
                energy -= 30;
                Breed(breed_number);
            }else
            if(move_type >= 9 && move_type <= 12 && energy >= 7)
            {
                energy -= 7;
                energy += Attack();
            }
            else
            {
                energy += 1;
            }
        }
        public int Attack()
        {
            if (CheckPosition(x + 1, y, true))
            {
                return GetCellFromCoord(x + 1, y).GetDamage(attack);
            }
            else
            if (CheckPosition(x, y + 1, true))
            {
                return GetCellFromCoord(x, y+1).GetDamage(attack);
            }
            else
            if (CheckPosition(x - 1, y, true))
            {
                return GetCellFromCoord(x - 1, y).GetDamage(attack);
            }
            else
            if (CheckPosition(x, y - 1, true))
            {
                return GetCellFromCoord(x, y-1).GetDamage(attack);
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
        public void Breed(int gen)
        {
            if(CheckPosition(x+1, y, false))
            {
                SimulationMain.buff.Add(new Cell(x+1, y, rn.Next(), gen, genom, defence, attack));
            }else
            if (CheckPosition(x, y+1, false))
            {
                SimulationMain.buff.Add(new Cell(x, y+1, rn.Next(), gen, genom, defence, attack));
            }else
            if (CheckPosition(x -1, y, false))
            {
                SimulationMain.buff.Add(new Cell(x-1, y, rn.Next(), gen, genom, defence, attack));
            }else
            if (CheckPosition(x, y-1, false))
            {
                SimulationMain.buff.Add(new Cell(x, y-1, rn.Next(), gen, genom, defence, attack));
            }
        }

        public void Move(int direction)
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
