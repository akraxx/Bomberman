using System;

namespace Bomberman
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// Point d’entrée principal pour l’application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BombermanGame game = new BombermanGame())
            {
                game.Run();
            }
        }
    }
#endif
}

