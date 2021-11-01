namespace Modulos.Testing
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Allows to customize test execution/creation.
    /// </summary>
    public interface ITestOptions
    {
        /// <summary>
        /// Wraps test with the specified wrapper.
        /// </summary>
        /// <typeparam name="TWrapper">Type of the wrapper.</typeparam>
        /// <returns>Test options instance.</returns>
        ITestOptions Wrap<TWrapper>() where TWrapper : ITestWrapper;
       
        /// <summary>
        /// Wraps test with the specified wrapper.
        /// </summary>
        /// <param name="wrapperType">Type of the wrapper. Must inherit from <see cref="ITestWrapper"/>.</param>
        /// <returns>Test options instance.</returns>
        ITestOptions Wrap(Type wrapperType);
        
        /// <summary>
        /// Removes specified wrapper from the test wrappers.
        /// </summary>
        /// <param name="wrapperType">Type of the wrapper. Must inherit from <see cref="ITestWrapper"/>.</param>
        /// <returns>Test options instance.</returns>
        ITestOptions Unwrap(Type wrapperType);
        
        /// <summary>
        /// Removes specified wrapper from the test wrappers.
        /// </summary>
        /// <typeparam name="TWrapper">Type of the wrapper.</typeparam>
        /// <returns>Test options instance.</returns>
        ITestOptions Unwrap<TWrapper>();
        
        /// <summary>
        /// Returns defined wrappers.
        /// </summary>
        /// <returns>Collection of defined wrappers.</returns>
        IEnumerable<Type> GetWrappers();
    }
}