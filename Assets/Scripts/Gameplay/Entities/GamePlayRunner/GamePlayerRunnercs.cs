using System.Linq;
using Framework.Entities;
using Framework.Entities.ContainerEntity;
using Framework.Entities.Variable.Boolean;
using Framework.Resolver;
using Gameplay.Entities.Stage;

namespace Gameplay.Entities.GamePlayRunner
{
    public interface IGamePlayRunner : IContainerEntity<IGamePlayRunnerData, IGamePlayRunnerSavedData>
    {
        void Play(IStage stage);
        void Stop();
        void Pause();
        void Resume();
        void Reset();
        IStage Stage { get; }
    }
  
    public class GamePlayRunner : ContainerEntity<IGamePlayRunnerData, IGamePlayRunnerSavedData>, IGamePlayRunner
    {
        public GamePlayRunner(IGamePlayRunnerData data, IGamePlayRunnerSavedData savedData)
            : base(data, savedData)
        {
        }

        private IBoolean _isStarted;
        private IBoolean _isPaused;
        private IBoolean _isEnded;

        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            _isStarted = GetComponentEntity<IBoolean>("is_started");
            _isPaused = GetComponentEntity<IBoolean>("is_paused");
            _isEnded = GetComponentEntity<IBoolean>("is_ended");
            Reset();
        }

        public void Play(IStage stage)
        {
            Stage = stage;
            if (!_isStarted.Value)
            {
                _isStarted.SetValue(true);
            }
        }

        public void Stop()
        {
            if (!_isEnded.Value)
            {
                _isEnded.SetValue(true);
            }
        }

        public void Pause()
        {
            if (!_isPaused.Value)
            {
                _isPaused.SetValue(true);
            }
        }

        public void Resume()
        {
            if (!_isPaused.Value)
            {
                _isPaused.SetValue(false);
            }
        }

        public void Reset()
        {
            _isStarted.SetValue(false);
            _isPaused.SetValue(false);
            _isEnded.SetValue(false);
        }

        public IStage Stage { get; private set; }
    }
}