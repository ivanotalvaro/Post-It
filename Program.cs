using System;
using System.Threading;
using System.Windows.Forms;

namespace Post_it
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "PostItDigitalAppMutex", out createdNew))
            {
                if (!createdNew)
                {
                    // La aplicación ya está ejecutándose
                    MessageBox.Show("La aplicación Post-it Digital ya está en ejecución.\n\nRevisa la bandeja del sistema.", 
                                  "Post-it Digital", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information);
                    return;
                }

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                
                try
                {
                    Application.Run(new Form1());
                }
                finally
                {
                    // El mutex se libera automáticamente al salir del bloque using
                }
            }
        }    
    }
}