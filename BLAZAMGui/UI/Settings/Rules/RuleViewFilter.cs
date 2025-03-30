using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.UI.Settings.Rules
{
    public class RuleViewFilter
    {

        private NotificationType _trigger;
        public NotificationType Trigger
        {
            get => _trigger; set
            {
                if (_trigger == value) return;
                _trigger = value;
                TriggerChanged?.Invoke();

            }
        }
        private ActiveDirectoryObjectType _objectType;
        public ActiveDirectoryObjectType ObjectType
        {
            get => _objectType; set
            {
                if (_objectType == value) return;
                _objectType = value;
                if (!Trigger.IsNotificationAppropriateForObject(value))
                {
                    Trigger = GetObjectTypeTriggers().First();
                }
                ObjectTypeChanged?.Invoke();
            }
        }
        private readonly List<NotificationType> _triggerTypes = new();
        public List<NotificationType> GetObjectTypeTriggers()
        {
            return _triggerTypes.Where(t => t.IsNotificationAppropriateForObject(ObjectType)).ToList();
        }

        public RuleViewFilter()
        {
            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                _triggerTypes.Add(type);
            }
            _triggerTypes = _triggerTypes.OrderBy(t => t.ToString()).ToList();
            _trigger = _triggerTypes.First();
        }

        public AppEvent ObjectTypeChanged { get; set; }
        public AppEvent TriggerChanged { get; set; }
    }
}
