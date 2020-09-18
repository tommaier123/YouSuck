using System;

namespace YouSuck
{
    internal class UpdateUIDelegate
    {
        private Action<string> updateText;

        public UpdateUIDelegate(Action<string> updateText)
        {
            this.updateText = updateText;
        }
    }
}