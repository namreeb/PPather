using System;
using System.Collections.Generic;
using System.Text;

using Pather.Activities;
using Pather.Parser;
using Pather.Graph;

namespace Pather.Tasks
{
    class WaitTask : ParserTask
    {
        bool started = false;
        string waitfor;
        Activity WaitActivity = null;

        public WaitTask(PPather pather, NodeTask node)
            : base(pather, node)
        {
            waitfor = node.GetValueOfId("For").GetStringValue();
        }

        public override Location GetLocation()
        {
            return null;
        }

        public override void GetParams(List<string> l)
        {
            l.Add("For");
            base.GetParams(l);
        }

        public override string ToString()
        {
            return "Waiting";
        }

        public override bool IsFinished()
        {
            return started;
        }

        public override bool WantToDoSomething()
        {
            return !started;
        }

        public override Activity GetActivity()
        {
            if (WaitActivity == null)
                WaitActivity = new ActivityWait(this, waitfor);
            return WaitActivity;
        }

        public override bool ActivityDone(Activity task)
        {
            started = true;
            return true;
        }
    }
}
