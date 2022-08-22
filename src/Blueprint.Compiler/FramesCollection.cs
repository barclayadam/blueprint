using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler
{
    /// <summary>
    /// A collection of <see cref="Frame" />s.
    /// </summary>
    public class FramesCollection : IReadOnlyList<Frame>
    {
        private readonly List<Frame> _frames = new List<Frame>();

        /// <inheritdoc />
        public int Count => this._frames.Count;

        /// <inheritdoc />
        public Frame this[int index] => this._frames[index];

        /// <summary>
        /// Adds the specified <see cref="Frame" /> to this collection.
        /// </summary>
        /// <param name="frame">The non-null Frame.</param>
        /// <exception cref="ArgumentNullException">If the frame is <c>null</c>.</exception>
        public void Add(Frame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            this._frames.Add(frame);
        }

        /// <summary>
        /// Inserts the specified <see cref="Frame" /> to this collection at the given index.
        /// </summary>
        /// <param name="index">The index at which to insert the frame.</param>
        /// <param name="frame">The non-null Frame.</param>
        /// <exception cref="ArgumentNullException">If the frame is <c>null</c>.</exception>
        public void Insert(int index, Frame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            this._frames.Insert(index, frame);
        }

        /// <summary>
        /// Finds the index of the given <see cref="Frame" />.
        /// </summary>
        /// <param name="frame">The non-null Frame.</param>
        /// <returns>Index of the given frame, or -1 if it cannot be found.</returns>
        /// <exception cref="ArgumentNullException">If the frame is <c>null</c>.</exception>
        public int IndexOf(Frame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            return this._frames.IndexOf(frame);
        }

        /// <summary>
        /// Adds all of the given frames to this collection.
        /// </summary>
        /// <param name="framesToAdd">The frames to add.</param>
        /// <exception cref="ArgumentNullException">If any one of the frames to add is <c>null</c>.</exception>
        public void AddRange(IEnumerable<Frame> framesToAdd)
        {
            foreach (var frame in framesToAdd)
            {
                if (frame == null)
                {
                    throw new ArgumentNullException(nameof(frame));
                }

                this._frames.Add(frame);
            }
        }

        /// <summary>
        /// Clears all existing frames from this collection.
        /// </summary>
        public void Clear()
        {
            this._frames.Clear();
        }

        /// <summary>
        /// Adds a ReturnFrame to the method that will return a variable of the specified type.
        /// </summary>
        /// <param name="returnType">The type of variable to be returned.</param>
        /// <returns>This frame collection.</returns>
        public FramesCollection Return(Type returnType)
        {
            var frame = new ReturnFrame(returnType);

            this.Add(frame);
            return this;
        }

        /// <summary>
        /// Adds a ReturnFrame for the specified variable.
        /// </summary>
        /// <param name="returnVariable">The variable to be returned.</param>
        /// <returns>This frame collection.</returns>
        public FramesCollection Return(Variable returnVariable)
        {
            var frame = new ReturnFrame(returnVariable);

            this.Add(frame);
            return this;
        }

        /// <summary>
        /// Adds a ConstructorFrame{T} to the method frames.
        /// </summary>
        /// <param name="constructor">An expression representing the call to a constructor.</param>
        /// <param name="configure">Optional, any additional configuration for the constructor frame.</param>
        /// <typeparam name="T">The type that will be constructed, inferred from the given expression.</typeparam>
        /// <returns>This frame collection.</returns>
        public FramesCollection CallConstructor<T>(Expression<Func<T>> constructor, Action<ConstructorFrame<T>> configure = null)
        {
            var frame = new ConstructorFrame<T>(constructor);
            configure?.Invoke(frame);
            this.Add(frame);

            return this;
        }

        /// <summary>
        /// Add a frame to the end by its type.
        /// </summary>
        /// <typeparam name="T">The type of frame to add.</typeparam>
        /// <returns>This frame collection.</returns>
        public FramesCollection Append<T>() where T : Frame, new()
        {
            return this.Append(new T());
        }

        /// <summary>
        /// Append one or more frames to the end.
        /// </summary>
        /// <param name="frames">The <see cref="Frame" />s to add.</param>
        /// <returns>This frame collection.</returns>
        public FramesCollection Append(params Frame[] frames)
        {
            this.AddRange(frames);
            return this;
        }

        /// <summary>
        /// Convenience method to add a method call to the GeneratedMethod Frames
        /// collection.
        /// </summary>
        /// <param name="expression">An expression representing a method call.</param>
        /// <param name="configure">Optional configuration of the MethodCall.</param>
        /// <typeparam name="T">The type from which to call a method.</typeparam>
        /// <returns>This frame collection.</returns>
        public FramesCollection Call<T>(Expression<Action<T>> expression, Action<MethodCall> configure = null)
        {
            var @call = MethodCall.For(expression);
            configure?.Invoke(@call);
            this.Add(@call);

            return this;
        }

        /// <summary>
        /// Writes this frame collection to the given writer, looping around all children and calling <see cref="Frame.GenerateCode" />.
        /// </summary>
        /// <param name="variables">The variable source.</param>
        /// <param name="method">The method being written.</param>
        /// <param name="writer">The source writer.</param>
        public void Write(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer)
        {
            foreach (var frame in this)
            {
                frame.GenerateCode(variables, method, writer);
            }
        }

        /// <inheritdoc />
        IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator()
        {
            return this._frames.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._frames).GetEnumerator();
        }
    }
}
