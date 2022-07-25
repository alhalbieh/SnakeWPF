using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeWPF.Models
{
    public class HighScore
    {
        public HighScore()
        {
        }

        public string Name { get; set; } = "";
        public int Score { get; set; }
    }
}