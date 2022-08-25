using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiniteStateMachine;
using System;

namespace StateMachineTest
{
    [TestClass]
    public class FiniteStateMachineTests
    {
        private enum TrafficLightsState
        {
            Green,
            Yellow,
            Red
        }

        FiniteStateMachine<TrafficLightsState> _sut = new FiniteStateMachine<TrafficLightsState>();

        [TestInitialize]
        public void TestsInit()
        {
            _sut = new FiniteStateMachine<TrafficLightsState>();
        }

        [TestMethod]
        public void CheckRegisterStateFailure()
        {
            _sut.RegisterState(TrafficLightsState.Green);
            Assert.ThrowsException<ArgumentException>(() => _sut.RegisterState(TrafficLightsState.Green));
            Assert.IsTrue(_sut.RegisteredStates.ContainsKey(TrafficLightsState.Green));
            Assert.AreEqual(_sut.RegisteredStates.Count, 1);
        }

        [TestMethod]
        public void CheckRegisterStateSuccess()
        {
            Action onEnterGreen = () => Console.WriteLine("onEnter green");
            Action onUpdateGreen = () => Console.WriteLine("onUpdate green");
            Action onLeaveGreen = () => Console.WriteLine("onLeave green");

            Action onEnterRed = () => Console.WriteLine("onEnter red");
            Action onUpdateRed = () => Console.WriteLine("onUpdate red");
            Action onLeaveRed = () => Console.WriteLine("onLeave red");

            _sut.RegisterState(TrafficLightsState.Green,
                onEnterGreen, onUpdateGreen, onLeaveGreen);

            _sut.RegisterState(TrafficLightsState.Red,
                onEnterRed, onUpdateRed, onLeaveRed);

            Assert.AreEqual(_sut.RegisteredStates.Count, 2);
            Assert.IsTrue(_sut.RegisteredStates.ContainsKey(TrafficLightsState.Green));
            Assert.IsTrue(_sut.RegisteredStates.ContainsKey(TrafficLightsState.Red));

            var greenState = _sut.RegisteredStates[TrafficLightsState.Green];
            Assert.AreEqual(TrafficLightsState.Green, greenState.Value);
            Assert.AreEqual(onEnterGreen, greenState.OnEnter);
            Assert.AreEqual(onUpdateGreen, greenState.OnUpdate);
            Assert.AreEqual(onLeaveGreen, greenState.OnLeave);

            var redState = _sut.RegisteredStates[TrafficLightsState.Red];
            Assert.AreEqual(TrafficLightsState.Red, redState.Value);
            Assert.AreEqual(onEnterRed, redState.OnEnter);
            Assert.AreEqual(onUpdateRed, redState.OnUpdate);
            Assert.AreEqual(onLeaveRed, redState.OnLeave);
        }

        [TestMethod]
        public void CheckTransitionFailure()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
                _sut.RegisterTransition(fromState: TrafficLightsState.Red,
                    toState: TrafficLightsState.Green, () => false));

            _sut.RegisterState(TrafficLightsState.Green);

            Assert.ThrowsException<InvalidOperationException>(() =>
                _sut.RegisterTransition(fromState: TrafficLightsState.Red,
                    toState: TrafficLightsState.Green, () => false));

            _sut.RegisterState(TrafficLightsState.Red);

            Assert.ThrowsException<ArgumentNullException>(() =>
                _sut.RegisterTransition(fromState: TrafficLightsState.Red,
                    toState: TrafficLightsState.Green, null));

            _sut.RegisterTransition(fromState: TrafficLightsState.Red,
                    toState: TrafficLightsState.Green, () => false);
        }

        [TestMethod]
        public void CheckTransitionSuccess()
        {
            _sut.RegisterState(TrafficLightsState.Green);
            _sut.RegisterState(TrafficLightsState.Red);

            Func<bool> condGreenToRed = () => true;
            _sut.RegisterTransition(fromState: TrafficLightsState.Green,
                toState: TrafficLightsState.Red, condGreenToRed);

            Func<bool> condRedToGreen = () => false;
            _sut.RegisterTransition(fromState: TrafficLightsState.Red,
                toState: TrafficLightsState.Green, condRedToGreen);

            //test _registeredStates[fromState].Transitions.Add(new Transition(_registeredStates[toState], condition));
            var greenState = _sut.RegisteredStates[TrafficLightsState.Green];
            var redState = _sut.RegisteredStates[TrafficLightsState.Red];

            Assert.AreEqual(1, greenState.Transitions.Count);
            Assert.AreEqual(1, redState.Transitions.Count);

            var toRedTrans = greenState.Transitions.Find(tr => tr.TargetState.Value == TrafficLightsState.Red);
            Assert.IsNotNull(toRedTrans);
            Assert.AreEqual(condGreenToRed, toRedTrans.Condition);
            Assert.AreEqual(TrafficLightsState.Red, toRedTrans.TargetState.Value);

            var toGreenTrans = redState.Transitions.Find(tr => tr.TargetState.Value == TrafficLightsState.Green);
            Assert.IsNotNull(toGreenTrans);
            Assert.AreEqual(condRedToGreen, toGreenTrans.Condition);
            Assert.AreEqual(TrafficLightsState.Green, toGreenTrans.TargetState.Value);
        }

        [TestMethod]
        public void CheckStartFSMFailure()
        {
            Assert.ThrowsException<InvalidOperationException>(() => _sut.Start(TrafficLightsState.Green));

            _sut.RegisterState(TrafficLightsState.Green);
            _sut.Start(TrafficLightsState.Green);
            Assert.ThrowsException<InvalidOperationException>(() => _sut.Start(TrafficLightsState.Green));
        }

        [TestMethod]
        public void CheckStartFSMSuccess()
        {
            Assert.IsFalse(_sut.IsStarted);

            bool onEnterInvoked = false;
            _sut.RegisterState(TrafficLightsState.Green, () => onEnterInvoked = true);
            _sut.Start(TrafficLightsState.Green);

            Assert.IsTrue(_sut.IsStarted);
            Assert.AreEqual(TrafficLightsState.Green, _sut.CurrentState);
            Assert.IsTrue(onEnterInvoked);
        }

        [TestMethod]
        public void CheckUpdateFSMFailure()
        {
            Assert.ThrowsException<InvalidOperationException>(() => _sut.Update());
        }

        [TestMethod]
        public void UpdateFsm()
        {
            bool onEnterGreen = false, onUpdateGreen = false, onLeaveGreen = false;
            bool onEnterYellow = false, onUpdateYellow = false, onLeaveYellow = false;
            bool onEnterRed = false, onUpdateRed = false, onLeaveRed = false;

            bool goToGreen = false, goToYellow = false, goToRed = false;

            _sut.RegisterState(TrafficLightsState.Green,
                () => { onEnterGreen = true; onUpdateGreen = false; onLeaveGreen = false; },
                () => { onEnterGreen = false; onUpdateGreen = true; onLeaveGreen = false; },
                () => { onEnterGreen = false; onUpdateGreen = false; onLeaveGreen = true; });

            _sut.RegisterState(TrafficLightsState.Yellow,
                () => { onEnterYellow = true; onUpdateYellow = false; onLeaveYellow = false; },
                () => { onEnterYellow = false; onUpdateYellow = true; onLeaveYellow = false; },
                () => { onEnterYellow = false; onUpdateYellow = false; onLeaveYellow = true; });

            _sut.RegisterState(TrafficLightsState.Red,
                () => { onEnterRed = true; onUpdateRed = false; onLeaveRed = false; },
                () => { onEnterRed = false; onUpdateRed = true; onLeaveRed = false; },
                () => { onEnterRed = false; onUpdateRed = false; onLeaveRed = true; });

            _sut.RegisterTransition(TrafficLightsState.Green, TrafficLightsState.Yellow, () => goToYellow);
            _sut.RegisterTransition(TrafficLightsState.Yellow, TrafficLightsState.Red, () => goToRed);
            _sut.RegisterTransition(TrafficLightsState.Red, TrafficLightsState.Green, () => goToGreen);

            _sut.Start(TrafficLightsState.Green);
            Assert.AreEqual(TrafficLightsState.Green, _sut.CurrentState);
            Assert.IsTrue(onEnterGreen);
            Assert.IsFalse(onUpdateGreen);
            Assert.IsFalse(onLeaveGreen);

            Assert.IsFalse(_sut.Update());
            Assert.IsFalse(onEnterGreen);
            Assert.IsTrue(onUpdateGreen);
            Assert.IsFalse(onLeaveGreen);

            goToYellow = true;
            Assert.IsTrue(_sut.Update());
            goToYellow = false;
            Assert.AreEqual(TrafficLightsState.Yellow, _sut.CurrentState);
            Assert.IsFalse(onEnterGreen);
            Assert.IsFalse(onUpdateGreen);
            Assert.IsTrue(onLeaveGreen);
            Assert.IsTrue(onEnterYellow);
            Assert.IsFalse(onUpdateYellow);
            Assert.IsFalse(onLeaveYellow);

            Assert.IsFalse(_sut.Update());
            Assert.IsFalse(_sut.Update());
            Assert.IsFalse(_sut.Update());

            Assert.AreEqual(TrafficLightsState.Yellow, _sut.CurrentState);
            Assert.IsFalse(onEnterGreen);
            Assert.IsFalse(onUpdateGreen);
            Assert.IsTrue(onLeaveGreen);
            Assert.IsFalse(onEnterYellow);
            Assert.IsTrue(onUpdateYellow);
            Assert.IsFalse(onLeaveYellow);

            goToRed = true;
            Assert.IsTrue(_sut.Update());
            goToRed = false;
            Assert.AreEqual(TrafficLightsState.Red, _sut.CurrentState);
            Assert.IsFalse(onEnterGreen);
            Assert.IsFalse(onUpdateGreen);
            Assert.IsTrue(onLeaveGreen);
            Assert.IsFalse(onEnterYellow);
            Assert.IsFalse(onUpdateYellow);
            Assert.IsTrue(onLeaveYellow);
            Assert.IsTrue(onEnterRed);
            Assert.IsFalse(onUpdateRed);
            Assert.IsFalse(onLeaveRed);

            Assert.IsFalse(_sut.Update());
            Assert.IsFalse(onEnterRed);
            Assert.IsTrue(onUpdateRed);
            Assert.IsFalse(onLeaveRed);

            goToGreen = true;
            Assert.IsTrue(_sut.Update());
            goToGreen = false;
            Assert.IsTrue(onEnterGreen);
            Assert.IsFalse(onUpdateGreen);
            Assert.IsFalse(onLeaveGreen);

            Assert.IsFalse(_sut.Update());
            Assert.IsFalse(onEnterGreen);
            Assert.IsTrue(onUpdateGreen);
            Assert.IsFalse(onLeaveGreen);
        }

        [TestCleanup]
        public void TestsCleanUp()
        {
        }
    }
}
