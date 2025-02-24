using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;

namespace WakeCommerce.ApiService.Controllers.Base
{
    public class ControllerBase : Controller
    {
        private readonly IMediatorHandler _mediatorHandler;
        private readonly DomainNotificationHandler _notifications;
        protected ControllerBase(INotificationHandler<DomainNotification> notifications,
                                IMediatorHandler mediatorHandler)
        {
            _notifications = (DomainNotificationHandler)notifications;
            _mediatorHandler = mediatorHandler;
        }

        protected bool OperacaoValida()
        {
            return !_notifications.TemNotificacao();
        }
        protected IEnumerable<string> ObterMensagensErro()
        {
            return _notifications.ObterNotificacoes().Select(c => c.Value).ToList();
        }

        public override BadRequestObjectResult BadRequest([ActionResultObjectValue] object? error)
        {
            var result = new ProblemDatails<object?>()
            {
                Detail = error,
                Status = StatusCodes.Status400BadRequest,
            };

            return base.BadRequest(result);
        }

        public override OkObjectResult Ok([ActionResultObjectValue] object? value)
        {
            var result = new SuccessResponse<object?>(value, true, StatusCodes.Status200OK);

            return base.Ok(result);
        }

        public ObjectResult Created([ActionResultObjectValue] object? value)
        {
            var result = new SuccessResponse<object?>(value, true, StatusCodes.Status200OK);

            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
