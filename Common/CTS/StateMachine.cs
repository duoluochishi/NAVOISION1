//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/17 13:14:44     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace NV.CT.CTS;

public class StateMachine<TState, TTrigger>
    where TState : notnull
    where TTrigger : notnull
{
    private TState _previewState;
    private TState _currentState;

    private IDictionary<TState, StateConfig<TState, TTrigger>> _configs;
    private IDictionary<TTrigger, Action<TState, TState, TTrigger, object>> _actions;

    private readonly ILogger _logger;

    public StateMachine(TState state) : this(state, null)
    {
    }

    public StateMachine(TState initState, ILogger logger)
    {
        _previewState = initState;
        _currentState = initState;
        _configs = new Dictionary<TState, StateConfig<TState, TTrigger>>();
        _actions = new Dictionary<TTrigger, Action<TState, TState, TTrigger, object>>();
        _logger = logger;
    }

    public TState Preview => _previewState;
    public TState Current => _currentState;

    public StateConfig<TState, TTrigger> Configure(TState state)
    {
        if (_configs.ContainsKey(state))
        {
            return _configs[state];
        }

        var config = new StateConfig<TState, TTrigger>
        {
            Current = state,
            Transitions = new Dictionary<TState, TTrigger>()
        };

        _configs.Add(state, config);

        return config;
    }

    public StateMachine<TState, TTrigger> Configure(TTrigger trigger, Action<TState, TState, TTrigger, object> action)
    {
        if (_actions.ContainsKey(trigger))
        {
            _actions[trigger] = action;
        }
        else
        {
            _actions.Add(trigger, action);
        }

        return this;
    }

    public void Next(TState state, object parameters)
    {
        //移除上层抛出的异常，仅通过日志排查
        if (!_configs.ContainsKey(_currentState))
        {
            _logger?.LogWarning($"(StateMachine) Not configure transitions of the current state: {_currentState}!");
            return;
        }

        if (!_configs[_currentState].Transitions.ContainsKey(state))
        {
            _logger?.LogWarning($"(StateMachine) Not configure to transition from {_currentState} to {state}!");
            //todo:待验证，保证后续业务正常
            _previewState = _currentState;
            _currentState = state;
            return;
        }

        if (_currentState.Equals(state))
        {
            _logger?.LogInformation($"(StateMachine) No state transition required: {_currentState}!");
            return;
        }

        _previewState = _currentState;
        _currentState = state;
        _logger?.LogInformation($"(StateMachine) Transitioned from {_previewState} to {_currentState}, and raised trigger: {_configs[_previewState].Transitions[_currentState]}.");

        var trigger = _configs[_previewState].Transitions[_currentState];
        if (_actions.ContainsKey(trigger))
        {
            var action = _actions[trigger];
            try
            {
                action?.Invoke(_previewState, _currentState, trigger, parameters);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, $"(StateMachine) Execute action exception: {ex.Message}");
            }
        }
    }
}