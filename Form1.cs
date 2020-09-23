using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArchiveText
{
    public partial class Form1 : Form
    {
        string text;
        List<int> byte_array = new List<int>();

        List<Symbol> symbols = new List<Symbol>();
        

        public Form1()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            symbols.Clear();
            openConfigurationFileToolStripMenuItem_Click(null, null);
            button2_Click(null, null);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            symbols.Clear();
            using (var fldrDlg = new OpenFileDialog())
            {
                if (fldrDlg.ShowDialog() == DialogResult.OK)
                {
                    FileInfo info = new FileInfo(fldrDlg.FileName);
                    fileWeight.Text = "Вес файла: " + info.Length + " байт";
                    text = File.ReadAllText(fldrDlg.FileName, Encoding.UTF8);
                    comboBox1.Enabled = true;
                }
            }
        }

        private void openByteArrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var fldrDlg = new OpenFileDialog())
            {
                if (fldrDlg.ShowDialog() == DialogResult.OK)
                {
                    byte[] array = File.ReadAllBytes(fldrDlg.FileName);
                    string code = "";
                    string current_code = "";
                    for (int i = 0; i < array.Length; i++)
                    {
                        BitArray bitArray = new BitArray(new[] { array[i] });
                        for (int j = 7; j>-1; j--)
                            if (bitArray[j] == true)
                                code += "1";
                            else code += "0";
                    }
                    
                    for (int i = 0; i < code.Length; i++)
                    {
                        foreach (Symbol s in symbols)
                            if (current_code.Equals(s.code))
                            {
                                Console.Write(current_code + " ");
                                text += s.symbol;
                                current_code = "";
                                break;
                            }
                        current_code += code[i];
                    }
                    
                    FileInfo info = new FileInfo(fldrDlg.FileName);
                    fileWeight.Text = "Вес файла: " + info.Length + " байт";
                    //byte_array = File.ReadAllBytes(fldrDlg.FileName);
                    comboBox1.Enabled = false;

                    ReadForm r = new ReadForm();
                    r.textBox1.Text = text;
                    r.Show();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (text != null)
                convert_text();
    
        }

        void convert_text()
        {
            for(int j = 0; j < text.Length; j++)
            {
                bool exist = false;
                for (int i = 0; i < symbols.Count; i++)

                    if (symbols[i].symbol==text.Substring(j,1))
                    {
                        symbols[i].ratio++;
                        exist = true;
                        break;
                    }

                if (exist == false)
                    symbols.Add(new Symbol(text.Substring(j, 1)));
                
            }
            float ratio_sum = 0;

            for (int i = 0; i < symbols.Count; i++)
            {
                symbols[i].ratio /= text.Length;
                ratio_sum += symbols[i].ratio;
            }
                
            List<Symbol> sorted = sort_list();

            for (int i = 0; i < symbols.Count; i=i+2) 
            {
                letter_array.Text += "(" + sorted[i].symbol + ") " + sorted[i].ratio + "     ";
                if (i != symbols.Count-1)
                    letter_array.Text += "(" + sorted[i+1].symbol + ") " + sorted[i+1].ratio + '\r' + '\n';
            }
                

            if (comboBox1.SelectedIndex == 0)
                shannon_fano(sorted, 0, symbols.Count - 1, ratio_sum, "");
            else if (comboBox1.SelectedIndex == 1)
                huffman(sorted);
            else if (comboBox1.SelectedIndex == 2)
                uniform_coding(sorted);

            for (int i = 0; i < symbols.Count; i=i+2) 
            {
                code_array.Text += "(" + sorted[i].symbol + ") " + sorted[i].code + "     ";
                if (i != symbols.Count - 1)
                    code_array.Text += "(" + sorted[i+1].symbol + ") " + sorted[i+1].code + '\r' + '\n';
            }    

            float entr = 0;
            for(int i = 0; i < sorted.Count; i++)
                entr += (float)(sorted[i].ratio * Math.Log(sorted[i].ratio, 2));
            entr = -entr;
            Entropia.Text = "Энтропия: " + entr;

            float med = 0;

            for (int i = 0; i < sorted.Count; i++)
                med += sorted[i].code.Length*sorted[i].ratio;

            medium.Text = "С.Д.К.С: " + med;
        }

        void uniform_coding(List<Symbol> list)
        {
            byte bit_count = 1;
            while (pow(2, bit_count) < list.Count)
                bit_count++;

            int current_code = 0;
            
            for(int i = 0; i < list.Count; i++)
            {
                string code = Convert.ToString(current_code, 2);
                string add = "";
                for (int j = 0; j <= bit_count - code.Length; j++)
                    add += "0";
                code = add + code;
                list[i].code = code;
                current_code++;
                Console.WriteLine(list[i].symbol + " " + list[i].code);
            }
        }

        public int pow(int value, int degree)
        {
            int result = value;
            for (int i = 0; i < degree; i++)
                result *= value;
            return result;
        }

        void huffman(List<Symbol> list)
        {
            List<List<Symbol>> combinations = new List<List<Symbol>>();
            for (int i = 0; i<list.Count; i++)
            {
                List<Symbol> l = new List<Symbol>();
                l.Add(list[i]);
                combinations.Add(l);
            }

            while (combinations.Count > 1)
            {
                List<Symbol> c1 = combinations[combinations.Count-1];
                List<Symbol> c2 = combinations[combinations.Count-2];
                List<Symbol> buffer = new List<Symbol>();

                for (int j = 0; j < c1.Count; j++)
                    c1[j].code = "0" + c1[j].code;
                for (int j = 0; j < c2.Count; j++)
                    c2[j].code = "1" + c2[j].code;

                buffer.AddRange(c1);
                buffer.AddRange(c2);
                combinations.Remove(c1);
                combinations.Remove(c2);
                combinations.Add(buffer);

                float[] sum = new float[combinations.Count];
                for (int v = 0; v < combinations.Count; v++)
                    for (int z = 0; z < combinations[v].Count; z++)
                        sum[v] += combinations[v][z].ratio;

                for (int v = 0; v < combinations.Count - 1; v++)
                    for (int z = v + 1; z < combinations.Count; z++)
                        if (sum[z] > sum[v])
                        {
                            List<Symbol> swap = combinations[v];
                            combinations[v] = combinations[z];
                            combinations[z] = swap;
                        }
            }
   
        }

        void shannon_fano(List<Symbol> list,int start_id, int end_id, float ratio_sum,string current_code)
        {
            if(end_id - start_id!=0)
            {
                float current_sum = 0;
                for (int i = start_id; i <= end_id; i++)
                {
                    current_sum += list[i].ratio;
                    if (current_sum >= ratio_sum / 2f)
                    {
                        shannon_fano(list, start_id, i, current_sum, current_code+"1");
                        shannon_fano(list, i+1, end_id, ratio_sum - current_sum, current_code + "0");
                        return;
                    }
                    
                }
            }
            else if(start_id< list.Count)
                list[start_id].code = current_code;  
        }

        List<Symbol> sort_list()
        {
            List<Symbol> copy = symbols.ToList();

            for(int i = 0; i < copy.Count-1; i++)
                for(int j = i+1; j< copy.Count; j++)
                {
                    if(copy[j].ratio > copy[i].ratio)
                    {
                        Symbol swap = copy[i];
                        copy[i] = copy[j];
                        copy[j] = swap;
                    }
                }

            return copy;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            using (var fldrDlg = new OpenFileDialog())
            {
                if (fldrDlg.ShowDialog() == DialogResult.OK)
                {
                    string res = "";

                    for (int i = 0; i < text.Length; i++)
                        for (int j = 0; j < symbols.Count; j++)
                            if (text.Substring(i,1).Equals(symbols[j].symbol))
                            {
                                res += symbols[j].code;
                                break;
                            }
                    
                    List<byte> bytes = new List<byte>();
                    byte current = 0;
                    byte current_id = 0;
                    
                    for (int i = 0; i < res.Length; i++)
                    {
                        if (current_id == 8)
                        {
                            bytes.Add(current);
                            current = 0;
                            current_id = 0;
                        }

                        if (res[i].Equals('1'))
                            current = (byte)((current << 1) + 1);
                        else
                            current = (byte)(current << 1);

                        current_id++;
                    }

                    ReadForm r = new ReadForm();
                    r.textBox1.Text = res;
                    r.Show();

                    File.WriteAllBytes(fldrDlg.FileName, bytes.ToArray());
                }
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var fldrDlg = new OpenFileDialog())
            {
                if (fldrDlg.ShowDialog() == DialogResult.OK)
                {
                    string res = "";

                    for (int j = 0; j < symbols.Count - 1; j++)
                        res += symbols[j].symbol + "=" + symbols[j].code+"|";
                    res += symbols[symbols.Count-1].symbol + "=" + symbols[symbols.Count - 1].code;
                    Console.WriteLine(symbols.Count);
                    File.WriteAllText(fldrDlg.FileName, res, Encoding.UTF8);
                }
            }
        }

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var fldrDlg = new OpenFileDialog())
            {
                if (fldrDlg.ShowDialog() == DialogResult.OK)
                {
                    string res = File.ReadAllText(fldrDlg.FileName, Encoding.UTF8);
                    string[] symbol = res.Split('|');
                    for(int i = 0; i < symbol.Length; i++)
                    {
                        string[] data = symbol[i].Split('=');

                        symbols.Add(new Symbol(data[0], data[1]));
                        
                    }
                }
            }
        }

    }
    public class Symbol
    {
        public string symbol;
        public float ratio;
        public string code;

        public Symbol(string s)
        {
            symbol = s;
            ratio = 1;
        }

        public Symbol(string s, string code)
        {
            symbol = s;
            this.code = code;
        }
    }


}
