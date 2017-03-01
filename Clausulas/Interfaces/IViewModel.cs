using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clausulas
{
    /// <summary>
    /// Interface para compartir datos de los ViewModel
    /// </summary>
    public interface IViewModel
    {
        void AcceptChanges();
        void CancelChanges();
    }
}
