namespace GYLib.Utils.ASEvents
{
    public class ASEvent
    {
        ///  ------------------------------------------
        /// 事件常量定义
        ///  ------------------------------------------

		/// <summary>
		/// 数据
		/// </summary>
		public object data;

        private string m_type;
        public string type
        {
            get{return m_type;}
        }

		private object m_sender;
		public object sender
        {
			get{return m_sender;}
        }

        private object m_currSender;

        public object currSender
        {
            get { return m_currSender;}
        }

        private bool m_bubbles;
        public bool bubbles { get { return m_bubbles;} }

        public bool needStopImmediatePropagation = false;
        public bool needStopPropagation = false;

        public ASEvent(string type, bool bubbles = false)
        {
            m_type = type;
            m_bubbles = bubbles;
        }

        // 请不要调用此方法
        public void __SetSender(object sender)
        {
            m_sender = sender;
        }

        // 请不要调用此方法
        public void __SetCurrSender(object sender)
        {
            m_currSender = sender;
        }

        public virtual ASEvent clone()
        {
            return new ASEvent(m_type);
        }

        public void ResetType(string type)
        {
            m_type = type;
        }
    }
}