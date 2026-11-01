using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace StudyConstructor.Scenario.Tests
{
    public class ScenarioTests
    {
        private readonly List<GameObject> created = new();
        private List<EventSource> sources;

        [SetUp]
        public void SetUp()
        {
            sources = new List<EventSource>();
            for (int i = 1; i <= 8; i++)
            {
                var go = new GameObject($"P{i}");
                created.Add(go);
                sources.Add(go.AddComponent<PlugMechanic>());
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in created.Where(g => g != null))
                Object.DestroyImmediate(go);
            created.Clear();
        }

        // ---------- Генерация ----------

        [Test]
        public void Build_NestedBlocks_CreatesHierarchy()
        {
            var flow = Build(Lines(
                "start_parallel",
                "  start_continuous", Exec(1, 1), Exec(1, 0), "  stop_continuous",
                "  start_continuous", "    start_parallel", Exec(2, 1), Exec(3, 1), "    stop_parallel", "  stop_continuous",
                "stop_parallel"), out var errors, out _);

            Assert.IsEmpty(errors);
            var group = flow.Groups().Single();
            var root = (FlowBlock)group.Children().Single();
            Assert.IsTrue(root.IsParallel);
            Assert.AreEqual(2, root.Children().Count());
            Assert.AreEqual(4, group.Steps().Count());
        }

        [Test]
        public void Build_Groups_AreSeparate()
        {
            var flow = Build(Lines(
                "start_group:Первая", Exec(1, 1), "stop_group",
                "start_group:Вторая", Exec(2, 1), Exec(3, 1), "stop_group"), out var errors, out _);

            Assert.IsEmpty(errors);
            CollectionAssert.AreEqual(new[] { "Первая", "Вторая" }, flow.Groups().Select(g => g.name));
            Assert.AreEqual(2, flow.Groups().Last().Steps().Count());
        }

        [Test]
        public void Build_RepeatedAction_GetsUniqueId()
        {
            var flow = Build(Lines(Exec(1, 1), Exec(1, 1)), out var errors, out _);

            Assert.IsEmpty(errors);
            CollectionAssert.AreEqual(new[] { "P_PF_3[P1]=1", "P_PF_3[P1]=1#2" }, StepIds(flow));
        }

        [Test]
        public void Build_NpzFormat_CommentsDefaultsAndNumbers()
        {
            var flow = Build(Lines(
                "\\\\\\Ветка 1",
                "start_continuous   // комментарий в конце строки",
                "  set_action_number:1.1_P",
                "  exec:P_PF_3*#P1",
                "  goto:point*#1*#3",
                "stop_continuous"), out var errors, out var warnings);

            Assert.IsEmpty(errors);
            Assert.That(warnings, Has.Some.StartsWith("Строка 5: команда «goto:point*#1*#3» пока не поддерживается"));
            var step = flow.Groups().Single().Steps().Single();
            Assert.AreEqual("P_PF_3[P1]=1", step.Id);
            Assert.AreEqual("1.1_P Открыть заглушку P1", step.Title);
        }

        [TestCase("start_parallel\n" + "exec:P_PF_3*#[P1]*#1", "Строка 1: start_parallel не закрыт")]
        [TestCase("start_parallel\nexec:P_PF_3*#[P1]*#1\nstop_continuous\nstop_parallel", "Строка 3: stop_continuous, а открыт start_parallel")]
        [TestCase("exec:P_PF_3*#[Нет]*#1", "Строка 1: на сцене нет объекта [Нет]")]
        [TestCase("exec:P_XX*#[P1]*#1", "Строка 1: механика P_XX не найдена")]
        [TestCase("exec:P_PF_3*#[P1]*#2", "Строка 1: значение «2» не подходит")]
        [TestCase("exec:P_PF_3", "Строка 1: ожидается exec:")]
        [TestCase("open P1", "Строка 1: неизвестная команда")]
        [TestCase("start_condition:P_PF_3*#[P1]\nexec:P_PF_3*#[P1]*#1", "Строка 1: команда «start_condition:P_PF_3*#[P1]» пока не поддерживается")]
        [TestCase("start_group:A\nexec:P_PF_3*#[P1]*#1\nstop_group\nexec:P_PF_3*#[P2]*#1", "Строка 4: команда вне группы")]
        [TestCase("exec:P_PF_3*#[P1]*#1*#if:[Нет]=1", "Строка 1: для условия нужен объект [Нет]")]
        [TestCase("exec:P_PF_3*#[P1]*#1*#if:P2", "Строка 1: условие «if:P2» должно выглядеть так")]
        [TestCase("exec:P_PF_3*#[P1]*#1*#0", "Строка 1: лишний параметр «0»")]
        [TestCase("forbid:P_PF_3*#[Нет]\nexec:P_PF_3*#[P1]*#1", "Строка 1: на сцене нет объекта [Нет]")]
        public void Build_Errors_HaveLineNumbers_AndCreateNothing(string text, string expected)
        {
            var flow = Build(text, out var errors, out _);

            Assert.IsNull(flow);
            Assert.That(errors, Has.Some.StartsWith(expected), string.Join("\n", errors));
            Assert.IsFalse(Object.FindObjectsByType<ScenarioFlow>(FindObjectsInactive.Include).Any(f => f.name == "Тест"));
        }

        // ---------- Активация ----------

        [Test]
        public void Run_ParallelThenContinuous_ActivatesInOrder()
        {
            var (run, _) = Start(Lines(
                "start_parallel", Exec(1, 1), Exec(2, 1), "stop_parallel",
                "start_continuous", Exec(3, 1), Exec(4, 1), "stop_continuous"), new TrainingRules());

            CollectionAssert.AreEquivalent(new[] { "P_PF_3[P1]=1", "P_PF_3[P2]=1" }, ActiveIds(run));
            Plug(2).UsePrimary();
            CollectionAssert.AreEquivalent(new[] { "P_PF_3[P1]=1" }, ActiveIds(run));
            Plug(1).UsePrimary();
            CollectionAssert.AreEquivalent(new[] { "P_PF_3[P3]=1" }, ActiveIds(run));
            Plug(3).UsePrimary();
            Plug(4).UsePrimary();
            Assert.IsTrue(run.IsFinished);
        }

        [Test]
        public void Run_ChainsInParallel_ProgressIndependently()
        {
            var (run, _) = Start(Lines(
                "start_parallel",
                "start_continuous", Exec(1, 1), Exec(1, 0), "stop_continuous",
                "start_continuous", Exec(2, 1), Exec(2, 0), "stop_continuous",
                "stop_parallel"), new TrainingRules());

            Plug(1).UsePrimary();
            CollectionAssert.AreEquivalent(new[] { "P_PF_3[P1]=0", "P_PF_3[P2]=1" }, ActiveIds(run));
        }

        [Test]
        public void Run_FollowsHierarchyOrder_AndSkipsDisabledSteps()
        {
            var flow = Build(Lines("start_continuous", Exec(1, 1), Exec(2, 1), Exec(3, 1), "stop_continuous"), out _, out _);
            var steps = flow.Groups().Single().Steps().ToList();
            steps[2].transform.SetAsFirstSibling();   // перетащили P3 наверх
            steps[1].gameObject.SetActive(false);     // выключили P2

            var (run, _) = StartFlow(flow, new TrainingRules());

            CollectionAssert.AreEqual(new[] { "P_PF_3[P3]=1" }, ActiveIds(run));
            Plug(3).UsePrimary();
            CollectionAssert.AreEqual(new[] { "P_PF_3[P1]=1" }, ActiveIds(run));
            Plug(1).UsePrimary();
            Assert.IsTrue(run.IsFinished);
        }

        [Test]
        public void Run_ActionOutOfOrder_IsViolationOfThatStep()
        {
            var (run, _) = Start(Lines("start_continuous", Exec(1, 1), Exec(2, 1), "stop_continuous"), new TrainingRules());

            Plug(2).UsePrimary();

            Assert.AreEqual(1, run.Steps[1].Violations);
            Assert.AreEqual(StepStatus.Inactive, run.Steps[1].Status);
        }

        [Test]
        public void Run_ActionOutOfOrder_IgnoredWhenPolicyIgnore()
        {
            var (run, _) = Start(Lines("start_continuous", Exec(1, 1), Exec(2, 1), "stop_continuous"),
                new TrainingRules(), OutOfOrderPolicy.Ignore);

            Plug(2).UsePrimary();

            Assert.AreEqual(0, run.Steps.Sum(s => s.Violations));
        }

        [Test]
        public void Run_IgnoreGroups_RunsAllGroupsAsOne()
        {
            var flow = Build(Lines(
                "start_group:A", "start_parallel", Exec(1, 1), Exec(2, 1), "stop_parallel", "stop_group",
                "start_group:B", Exec(3, 1), "stop_group"), out _, out _);

            var (run, _) = StartFlow(flow, new TrainingRules(), ignoreGroups: true);

            Assert.AreEqual(1, run.Groups.Count);
            Assert.AreEqual(3, run.Groups[0].Steps.Count);
            CollectionAssert.AreEquivalent(new[] { "P_PF_3[P1]=1", "P_PF_3[P2]=1" }, ActiveIds(run));
            Plug(1).UsePrimary();
            Plug(2).UsePrimary();
            CollectionAssert.AreEqual(new[] { "P_PF_3[P3]=1" }, ActiveIds(run));
        }

        [Test]
        public void Run_IfCondition_IsViolationUntilOtherObjectInState()
        {
            var (run, _) = Start(Exec(1, 0) + "*#if:[P2]=1", new TrainingRules());
            Assert.AreEqual("Закрыть заглушку P1 (когда заглушка P2 открыта)", run.Steps[0].Step.Title);

            Plug(1).UseSecondary();          // P2 закрыта — нельзя
            Assert.AreEqual(1, run.Steps[0].Violations);
            Assert.AreEqual(StepStatus.Active, run.Steps[0].Status);

            Plug(2).UsePrimary();            // открыли P2 (в сценарии этого шага нет — не учитывается)
            Plug(1).UseSecondary();
            Assert.IsTrue(run.IsFinished);
            Assert.AreEqual(1, run.Steps.Sum(s => s.Violations));
        }

        [Test]
        public void Run_Forbid_GivesOneViolationPerAction_AndIsNotOutOfOrder()
        {
            var (run, _) = Start(Lines(
                "forbid:P_PF_3*#[P8]*#Заглушку 8 трогать нельзя",
                "start_parallel", Exec(1, 1), Exec(2, 1), "stop_parallel",
                Exec(3, 1)), new TrainingRules());
            var reasons = new List<string>();
            run.ViolationRegistered += (_, reason) => reasons.Add(reason);

            Plug(8).UsePrimary();

            CollectionAssert.AreEqual(new[] { "Заглушку 8 трогать нельзя" }, reasons);
            Assert.AreEqual(0, run.Steps[2].Violations);
        }

        [Test]
        public void Run_ForbidInBlock_WorksOnlyWhileBlockIsActive()
        {
            var (run, _) = Start(Lines(
                "start_continuous", "forbid:P_PF_3*#[P8]", Exec(1, 1), "stop_continuous",
                Exec(2, 1)), new TrainingRules());

            Plug(8).UsePrimary();
            Assert.AreEqual(1, run.Steps[0].Violations);

            Plug(1).UsePrimary();
            Plug(8).UsePrimary();
            Assert.AreEqual(0, run.Steps[1].Violations);
        }

        // ---------- Механики P_BT_1 и P_TC_1 ----------

        [Test]
        public void Button_Press_CompletesStep()
        {
            var button = Add<ButtonMechanic>("КН-1");
            var (run, _) = Start("exec:P_BT_1*#[КН-1]", new TrainingRules());
            Assert.AreEqual("Нажать кнопку КН-1", run.Steps[0].Step.Title);

            button.UsePrimary();

            Assert.IsTrue(run.IsFinished);
        }

        [Test]
        public void Zone_ExitWhileEnterNeeded_IsNotViolation()
        {
            var zone = Add<ZoneMechanic>("Выход");
            var player = Player();
            var (run, _) = Start("exec:P_TC_1*#[Выход]*#1", new TrainingRules());

            zone.SendMessage("OnTriggerEnter", player);
            Assert.IsTrue(run.IsFinished);

            (run, _) = Start("exec:P_TC_1*#[Выход]*#1", new TrainingRules());
            zone.SendMessage("OnTriggerExit", player);
            Assert.AreEqual(0, run.Steps[0].Violations);
            Assert.AreEqual(StepStatus.Active, run.Steps[0].Status);
        }

        [Test]
        public void Zone_StateWorksInCondition_AndForbidWithValueBlocksOnlyEnter()
        {
            var zone = Add<ZoneMechanic>("Площадка");
            var danger = Add<ZoneMechanic>("Опасная зона");
            var player = Player();
            var (run, _) = Start(Lines(
                "forbid:P_TC_1*#[Опасная зона]*#1*#Нельзя",
                Exec(1, 1) + "*#if:[Площадка]=1"), new TrainingRules());

            Plug(1).UsePrimary();                           // вне площадки — нарушение
            danger.SendMessage("OnTriggerEnter", player);   // запрещённый вход
            danger.SendMessage("OnTriggerExit", player);    // выход не запрещён
            Assert.AreEqual(2, run.Steps[0].Violations);

            zone.SendMessage("OnTriggerEnter", player);
            Plug(1).UsePrimary();
            Assert.IsTrue(run.IsFinished);
        }

        // ---------- Контрольные примеры ----------

        [Test]
        public void Training_FirstSecondThirdAttempt_Gives10_5_0()
        {
            var (_, recorder) = Start(Lines("start_parallel", Exec(1, 1), Exec(2, 1), Exec(3, 1), "stop_parallel"),
                new TrainingRules());

            Plug(1).UsePrimary();                                                  // с первой попытки
            Plug(2).UseSecondary(); Plug(2).UsePrimary();                          // со второй
            Plug(3).UseSecondary(); Plug(3).UseSecondary(); Plug(3).UsePrimary();  // с третьей

            var report = new TrainingScoreAnalyzer().Analyze(recorder.Log);
            CollectionAssert.AreEqual(new[] { 10, 5, 0 }, report.Steps.Select(s => s.Points));
            Assert.AreEqual(15, report.Score);
            Assert.AreEqual(30, report.MaxScore);
        }

        [Test]
        public void Exam_TwoViolationsInGroupOfFour_SkipsRest_Gives70()
        {
            var (run, recorder) = Start(Lines(
                "start_group:Группа 1", "start_parallel", Exec(1, 1), Exec(2, 1), Exec(3, 1), Exec(4, 1), "stop_parallel", "stop_group",
                "start_group:Группа 2", Exec(5, 1), "stop_group"), new ExamRules(2));

            Plug(1).UseSecondary();
            Plug(2).UseSecondary();

            CollectionAssert.AreEqual(
                new[] { StepStatus.Failed, StepStatus.Failed, StepStatus.Skipped, StepStatus.Skipped, StepStatus.Active },
                run.Steps.Select(s => s.Status));

            Plug(5).UsePrimary();
            Assert.IsTrue(run.IsFinished);
            Assert.AreEqual(70, new ExamPenaltyAnalyzer().Analyze(recorder.Log).Score);
        }

        // ---------- Вспомогательное ----------

        private PlugMechanic Plug(int index) => (PlugMechanic)sources[index - 1];

        private T Add<T>(string name) where T : EventSource
        {
            var go = new GameObject(name);
            created.Add(go);
            go.AddComponent<BoxCollider>().isTrigger = true;
            var source = go.AddComponent<T>();
            sources.Add(source);
            return source;
        }

        private Collider Player()
        {
            var go = new GameObject("Player") { tag = "Player" };
            created.Add(go);
            return go.AddComponent<BoxCollider>();
        }

        private static string Exec(int plug, int value) => $"exec:P_PF_3*#[P{plug}]*#{value}";

        private static string Lines(params string[] lines) => string.Join("\n", lines);

        private static IEnumerable<string> ActiveIds(ScenarioRun run) => run.ActiveSteps.Select(s => s.Step.Id);

        private static IEnumerable<string> StepIds(ScenarioFlow flow) => flow.Groups().SelectMany(g => g.Steps()).Select(s => s.Id);

        private ScenarioFlow Build(string text, out List<string> errors, out List<string> warnings)
        {
            errors = new List<string>();
            warnings = new List<string>();
            var flow = ScriptParser.Build(text, "Тест", sources, errors, warnings);
            if (flow != null)
                created.Add(flow.gameObject);
            return flow;
        }

        private (ScenarioRun run, RunRecorder recorder) Start(string text, ExecutionRules rules,
            OutOfOrderPolicy policy = OutOfOrderPolicy.Violation)
        {
            var flow = Build(text, out var errors, out _);
            Assert.IsEmpty(errors, string.Join("\n", errors));
            return StartFlow(flow, rules, policy);
        }

        private (ScenarioRun run, RunRecorder recorder) StartFlow(ScenarioFlow flow, ExecutionRules rules,
            OutOfOrderPolicy policy = OutOfOrderPolicy.Violation, bool ignoreGroups = false)
        {
            var mode = new ScenarioMode("Тест", rules, null);
            mode.SetOutOfOrder(policy);
            mode.SetIgnoreGroups(ignoreGroups);
            var run = new ScenarioRun(mode, flow);
            var recorder = new RunRecorder(run, new List<ScenarioLogger>());
            foreach (var source in sources)
                source.Raised += run.Handle;
            run.Start();
            return (run, recorder);
        }
    }
}
