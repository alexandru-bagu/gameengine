using OpenTK;
using GameEngine.Collections;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
	public class Container : Drawable, IContainer
	{
		public virtual bool Sealed => false;
		public ReadOptimizedList<Drawable> Objects { get; set; }

		public Container()
		{
			Objects = new ReadOptimizedList<Drawable>();
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();
			foreach (var obj in Objects)
				obj.InternalUpdateLayout();
		}

        protected override void Draw()
        {
            if (Objects.Count > 0)
            {
                if (ClipContents)
                    Manager?.Scissors.Clip(ClientArea);

                if (Texture != null) Painter.Texture(Texture, ClientArea, Color.White);
                DrawObjects();

                if (ClipContents)
                    Manager?.Scissors.Unclip();
            }
        }

        public virtual void DrawObjects()
        {
            foreach (var obj in Objects)
                obj.InvokeDraw();
        }

		public override void Tick()
		{
			foreach (var obj in Objects)
				obj.Tick();
		}

		public virtual void Add(Drawable drawable)
		{
			if (Sealed) return;
			drawable.Parent = this;
			Objects.Add(drawable);

			InternalUpdateLayout();
		}

		public virtual void Remove(Drawable drawable)
		{
			if (Sealed) return;
			drawable.Parent = null;
			Objects.Remove(drawable);

			InternalUpdateLayout();
		}

		public virtual void Clear()
		{
			if (Sealed) return;
			Objects.Clear();

			InternalUpdateLayout();
		}

		public virtual void BringToFront(Drawable drawable)
		{
			if (drawable.Parent == this)
			{
				Objects.Remove(drawable);
				Objects.Add(drawable);
			}
		}

		public virtual void SendToBack(Drawable drawable)
		{
			if (drawable.Parent == this)
			{
				Objects.Remove(drawable);
				Objects.Insert(0, drawable);
			}
		}

		public override Drawable PickElement(Vector2 position)
		{
			for (int i = Objects.Count - 1; i >= 0; i--)
			{
				var obj = Objects[i];
				if (obj.Pick(position))
				{
					if (obj is IContainer)
						return ((IContainer)obj).PickElement(position);
					return obj;
				}
			}
			return this;
		}

		public virtual IContainer PickContainer(Vector2 position, Drawable exclude)
		{
			for (int i = Objects.Count - 1; i >= 0; i--)
			{
				var obj = Objects[i];
				if (obj.Pick(position) && (obj is IContainer) && exclude != obj)
				{
					var container = (IContainer)obj;
					if (!container.Sealed)
						return container.PickContainer(position, exclude);
				}
			}
			return this;
		}
	}
}
