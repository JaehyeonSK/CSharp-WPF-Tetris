using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.Classes
{
    public class SoundEffecter
    {
        public static void Play(UnmanagedMemoryStream soundResource)
        {
            var sound = new SoundPlayer(soundResource);
            sound.Play();
        }
    }
}
