using Rayiah.Objects.Interfaces;
using System;
using System.Threading.Tasks;

namespace Rayiah.Objects.Abstracts
{
    public abstract class RayiahObject : IDisposable, ISave
    {
        protected virtual bool AutoSave => false;

        public virtual string Name => GetType().Name;

        protected RayiahObject()
        {
            if(AutoSave)
                ISave.AddObject(this);
        }

        #region Print methods
        public virtual void Printl(string m, string origin, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = color;
            Console.Write($"[{GetType().Name}::{origin}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(m + '\n');
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public virtual void Printl(string m, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = color;
            Console.Write($"[{GetType().Name}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(m + '\n');
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public virtual void Printl(Exception m, ConsoleColor color = ConsoleColor.Cyan) => Printl(m.Message, color);

        public virtual void Printl(Exception m, string origin, ConsoleColor color = ConsoleColor.Cyan) => Printl(m.Message, origin, color);

        /// <summary>
        /// Printl, ale bez dopisku o obiekcie.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="color"></param>
        public virtual void PrintC(string m, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(m);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        #endregion

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Musi być asynchroniczne.
        /// </summary>
        /// <returns></returns>
        public virtual Task InitializeAsync() { return Task.CompletedTask; }
    }
}
