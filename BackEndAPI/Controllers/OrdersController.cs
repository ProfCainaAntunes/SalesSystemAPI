using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEndAPI.Data;
using BackEndAPI.Models;
using BackEndAPI.DTOs;

namespace BackEndAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly BackEndAPIContext _context;

        public OrdersController(BackEndAPIContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrder()
        {
            return await _context.Order.Include(o=>o.Items).ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Order.Include(o=>o.Items).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/Orders/bySellerId/5
        [HttpGet("bySellerId/{sellerId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderbySellerId(int sellerId)
        {
            var orders = await _context.Order
                                    .Include(o => o.Items)
                                    .Where(o => o.SellerId == sellerId)
                                    .ToListAsync();

            if (orders.Count == 0)
                return NotFound();

            return orders;
        }

        // GET: api/Orders/byClientId/5
        [HttpGet("byClientId/{clientId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderbyClientId(int clientId)
        {
            var orders = await _context.Order
                                    .Include(o => o.Items)
                                    .Where(o => o.SellerId == clientId)
                                    .ToListAsync();

            if (orders.Count == 0)
                return NotFound();

            return orders;
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, OrderDTO orderDTO)
        {
            // Verifica se Order existe no banco e a recupera.
            Order? order = await _context.Order
                                .Include(o=>o.Items)
                                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return BadRequest($"OrderId: {id} not found.");

            // Atualiza o Status da Order.
            order.Status = order.Status;

            // Adicionar novos itens no pedido.
            foreach (OrderItemDTO o in orderDTO.Items)
            {
                // Busca o produto a ser associado ao ItemPedido
                Product? product = await _context.Product.FindAsync(o.ProductId);

                // Verifica se o produto existe no banco.
                if (product == null)
                    return NotFound($"ProductId:{o.ProductId} not found.");

                // Verifica se há quantidade suficiente de produto para o pedido
                if (product.StockQuantity < o.Quantity || product.StockQuantity <= 0)
                    return BadRequest($"Insufficient stock for ProductId: {product.Id}");

                // Cria um novo item pedido.
                OrderItem item = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = o.ProductId,
                    Quantity = o.Quantity
                };

                // Calcula o valor do subtotal do item com base no preço do produto.
                item.CalculateSubtotal(product.Price);

                // Adiciona o novo item na lista de OrderItem de Order.
                order.Items.Add(item);

                // Atualiza a quantidade de produto no estoque
                product.Consume(o.Quantity);

                // Sava todas as mudanças
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            order.CalculateTotal();

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderDTO orderDTO)
        {
            // Verifica se os ID's do vendor e do cliente existem no banco de dados.
            Seller? seller = await _context.Seller.FindAsync(orderDTO.SellerId);
            Client? client = await _context.Client.FindAsync(orderDTO.ClientId);

            if (seller == null)
                return NotFound($"SellerId: {orderDTO.SellerId} not found.");

            if (client == null)
                return NotFound($"ClientId: {orderDTO.ClientId} not found.");

            // Cria um novo pedido.
            Order order = new Order
            {
                SellerId = orderDTO.SellerId,
                ClientId = orderDTO.ClientId,
                Status = orderDTO.Status
            };
            
            // Salva o pedido no banco para que seu ID sejá gerado.
            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            // Percorre todos os itens recebidos na requisição
            foreach (OrderItemDTO o in orderDTO.Items)
            {
                // Verifica se o produto associado ao item pedido existe no banco.
                Product? product = await _context.Product.FindAsync(o.ProductId);

                if (product == null)
                    return NotFound($"ProductId:{o.ProductId} not found.");

                // Verifica se há quantidade suficiente de produto para o pedido
                if (product.StockQuantity < o.Quantity || product.StockQuantity <= 0)
                    return BadRequest($"Insufficient stock for ProductId: {product.Id}");

                // Cria um novo item pedido.
                OrderItem item = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = o.ProductId,
                    Quantity = o.Quantity
                };

                // Calcula o valor do subtotal do item com base no preço do produto.
                item.CalculateSubtotal(product.Price);

                // Adiciona o novo item na lista de OrderItem de Order.
                order.Items.Add(item);

                // Atualiza a quantidade de produto no estoque
                product.Consume(o.Quantity);

                // Sava todas as mudanças
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            
            order.CalculateTotal();

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
        }
    }
}
