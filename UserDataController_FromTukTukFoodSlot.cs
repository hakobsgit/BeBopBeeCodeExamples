using System;
using Data.Enums;
using Data.Models;
using Data.SocketMessages.Core;
using Data.SocketMessages.Responses;
using Processors;
using Services;
using UniRx;
using Zenject;

namespace Controllers {
	public class UserDataController : IInitializable, IDisposable {
		[Inject] private ReactiveProperty<UserData> _userData;
		[Inject] private WebSocketService _webSocketService;
		[Inject] private JsonProcessor _jsonProcessor;
		[Inject] private ProtobufProcessor _protobufProcessor;

		private IDisposable _connectionObserverDisposable;

		public void Initialize() {
			_webSocketService.MessageReceived += OnMessageReceived;
			_webSocketService.DataReceived += OnDataReceived;
		}

		private void OnMessageReceived(ActionType type, string message) {
			if (type == ActionType.UserInfo) {
				_userData.Value = _jsonProcessor.Deserialize<UserResponse>(message).Data;
			}
		}

		private void OnDataReceived(ActionType type, byte[] message) {
			if (type == ActionType.UserInfo) {
				_userData.Value = _protobufProcessor.Deserialize<UserResponse>(message).Data;
			}
		}

		public void Dispose() {
			_connectionObserverDisposable?.Dispose();
		}
	}
}