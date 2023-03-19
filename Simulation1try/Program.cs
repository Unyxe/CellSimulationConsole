using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        static bool log_file = false;
        public static int life_span = 100;
        public static int normal_energy = 200;
        public static bool init_done = false;
        static void Main(string[] args)
        {
            Thread fps_thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    actual_fps = frames;
                    frames = 0;
                    if (init_done)
                    {
                        try
                        {
                            //DisplayField(0);
                        }
                        catch { }
                    }
                }
            });
            fps_thread.Start();
            Console.WriteLine(-67 % 60);
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

                cells.Add(new Cell(0, 0, 7, 8, random.Next(), 0, normal_energy, life_span));

                cells.Add(new Cell(0, 0, 27, 8, random.Next(), 0, normal_energy, life_span));


                init_done = true;
                while (true)
                {
                    if (log_file)
                    {
                        writer.WriteLine(cells.Count());
                    }
                    foreach(Cell cell in cells)
                    {
                        cell.x += width;
                        cell.x %= width;

                        cell.y += height;
                        cell.y %= height;
                    }
                    foreach (Cell cell in buff)
                    {
                        cell.x += width;
                        cell.x %= width;

                        cell.y += height;
                        cell.y %= height;
                    }
                    foreach (Cell cell in to_remove)
                    {
                        cell.x += width;
                        cell.x %= width;

                        cell.y += height;
                        cell.y %= height;
                    }
                    RefreshArrayCellPosition();
                    DisplayField(0);

                    foreach (Cell cell in cells)
                    {
                        cell.NextMove();
                        if (cell.health <= 0)
                        {
                            to_remove.Add(cell);
                        }
                        else if(cell.cell_type == 0 && cell.energy < -5)
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
                    int count_norm_cells = 0;
                    foreach(Cell c in cells)
                    {
                        if(c.cell_type == 0 || c.cell_type == 3)
                        {
                            count_norm_cells++;
                        }
                    }
                    if (count_norm_cells == 0)
                    {
                        break;
                    }
                    frames++;
                }
                writer.Close();
            }
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
            int[] cell_type = new int[4] {0, 0, 0, 0 };
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
                        Cell current_cell = GetCellFromCoord(j, i);
                        if(current_cell == null)
                        {
                            continue;
                        }
                        int cell_type_current = current_cell.cell_type;
                        cell_type[cell_type_current]++;
                        
                        switch (cell_type_current)
                        {
                            case 0:
                                Console.Write("C|");
                                break;
                            case 1:
                                Console.Write("L|");
                                break;
                            case 2:
                                Console.Write("w|");
                                break;
                            case 3:
                                Console.Write("S|");
                                break;
                        }
                        
                    }
                    
                }
                Console.WriteLine();
            }
            Console.WriteLine("\nPopulation: " + cells.Count);
            Console.WriteLine("Generation: " + generation);
            Console.WriteLine("FPS: " + actual_fps);

            Console.WriteLine("\nNormal cells: " + cell_type[0]);
            Console.WriteLine("Leaf cells: " + cell_type[1]);
            Console.WriteLine("Wood cells: " + cell_type[2]);
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
                //Console.WriteLine(cell.x + " " + cell.y);
                field[cell.y][cell.x][0] = 1;
                
            }
        }
        static Cell GetCellFromCoord(int x_w, int y_w)
        {
            foreach (Cell cell in cells)
            {
                if (cell.x == x_w && cell.y == y_w)
                {
                    return cell;
                }
            }
            return null;
        }
    }

    class Cell
    {
        public int cell_id;
        public int cell_type;
        public int parent_cell_id;

        // 0 - normal cell
        // 1 - leaf
        // 2 - wood
        // 3 - seed

        public int x;
        public int y;

        public double speed;
        public double defence = 10;
        public double attack = 20;

        public double health;
        public double energy;

        public static int genom_length = 128;
        public static int genom_options = 4;
        public static int cell_types_count = 4;

        public int[][] genom = new int[genom_length][];

        public int active_gen = 0;
        Random rn;

        public Cell(int cell_type_, int parent_cell_id_, int x_, int y_, int seed, int active_gen_, double energy_, double health_)
        {
            energy = energy_;
            health = health_;
            parent_cell_id = parent_cell_id_;
            cell_type = cell_type_;
            x = x_;
            y = y_;
            active_gen = active_gen_;
            rn = new Random(seed);
            cell_id = rn.Next();
            for (int i = 0; i < genom_length; i++)
            {
                genom[i] = new int[genom_options];
                genom[i][0] = rn.Next() % genom_length;
                genom[i][1] = rn.Next() % 4;
                genom[i][2] = rn.Next() % genom_length;
                genom[i][3] = rn.Next() % cell_types_count;
            }
        }

        public Cell(int cell_type_, int parent_cell_id_, int x_, int y_, int seed, int active_gen_, int[][] genom_, double defence_, double attack_, double energy_, double health_)
        {
            energy = energy_;
            health = health_;
            parent_cell_id = parent_cell_id_;
            cell_type = cell_type_;
            x = x_;
            y = y_;
            defence = defence_;
            attack = attack_;
            active_gen = active_gen_;
            rn = new Random(seed);
            cell_id = rn.Next();
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
                        m_v = rn.Next() % 4;
                        break;
                    case 2:
                        m_v = rn.Next() % genom_length;
                        break;
                    case 3:
                        m_v = rn.Next() % cell_types_count;
                        break;
                }
                
            }
            int j = rn.Next();
            if (j % 1000 < 100)
            {
                if (j % 2 == 0) 
                {
                    defence += rn.Next() % 21 - 10;
                    if(defence < 1)
                    {
                        defence = 1;
                    }
                }
                else
                {
                    attack += rn.Next() % 21 - 10;
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
            health -= 1;
            switch (cell_type)
            {
                case 0:
                    DoCellMove(count_how_many);
                    break;
                case 1:
                    Cell parent = GetCellFromID(parent_cell_id);
                    if(parent != null)
                    {
                        parent.GetEnergy(15);
                    }
                    break;
                case 2:
                    break;
                case 3:
                    DoSeedCheck();
                    break;
            }

        }
        public void DoCellMove(int how_many)
        {
            
            int next_gen = genom[active_gen][0];
            int breed_direction = genom[active_gen][1];
            int breed_number = genom[active_gen][2];
            int cell_type = genom[active_gen][3];
            energy--;
            if (energy > 10)
            {
                energy -= 10;
                Breed(breed_number, breed_direction, cell_type);
            }
            active_gen = next_gen;
        }
        public void DoSeedCheck()
        {
            if(GetCellFromID(parent_cell_id) == null)
            {
                cell_type = 0;
            }
        }
        public void GetEnergy(double energy_)
        {
            if(cell_type == 0)
            {
                energy += energy_;
                return;
            }
            Cell parent = GetCellFromID(parent_cell_id);
            if(parent != null)
            {
                parent.GetEnergy(energy_);
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
        public Cell GetCellFromID(int id)
        {
            foreach (Cell cell in SimulationMain.cells)
            {
                if (cell.cell_id == id)
                {
                    return cell;
                }
            }
            return null;
        }
        public bool Breed(int gen, int direction, int type)
        {
            int target_x = x;
            int target_y = y;
            switch (direction)
            {
                case 1:
                    target_x++;
                    break;
                case 2:
                    target_y++;
                    break;
                case 3:
                    target_x--;
                    break;
                case 4:
                    target_y--;
                    break;
            }
            if (CheckPosition(target_x, target_y, false))
            {
                if(type == 0)         //Moving cell
                {
                    SimulationMain.buff.Add(new Cell(0, cell_id, target_x, target_y, rn.Next(), gen, genom, defence, attack, energy, health));
                    SimulationMain.to_remove.Add(this);
                    SimulationMain.buff.Add(new Cell(2, cell_id, x, y, rn.Next(), gen, genom, defence, attack, SimulationMain.normal_energy, SimulationMain.life_span));
                }
                else if(type != 2)
                {
                    SimulationMain.buff.Add(new Cell(type, cell_id, target_x, target_y, rn.Next(), gen, genom, defence, attack, SimulationMain.normal_energy, SimulationMain.life_span));
                }
                
            }
            else { return false; }
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
            w_x %= SimulationMain.width;
            w_y %= SimulationMain.height;
            if ((w_x < SimulationMain.width && w_y < SimulationMain.height && w_x >= 0 && w_y >= 0) || true)
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
