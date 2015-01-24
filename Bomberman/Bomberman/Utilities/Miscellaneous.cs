using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Utilities
{
    static class Miscellaneous
    {
        public static string GetTimerString(TimeSpan timer)
        {
            if (timer >= TimeSpan.Zero)
            {
                int minutes = timer.Minutes;
                int seconds = timer.Seconds;
                return minutes.ToString() + " : " + (seconds < 10 ? "0" : "") + seconds.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}