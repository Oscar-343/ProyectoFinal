// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ---------------------------------------------------------------
// Arrastrar para desplazar tablas anchas (Materiales, Modelos, Pedidos).
// Antes solo se podía mover la tabla a la derecha usando la barra de
// scroll nativa, que queda pegada debajo de la tabla (hay que bajar
// toda la página para alcanzarla). Con esto se puede hacer clic y
// arrastrar desde cualquier parte visible de la tabla para moverla.
// ---------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.tabla-responsive').forEach(function (contenedor) {
        let arrastrando = false;
        let inicioX = 0;
        let scrollInicial = 0;

        contenedor.style.cursor = 'grab';

        contenedor.addEventListener('mousedown', function (e) {
            arrastrando = true;
            contenedor.classList.add('arrastrando');
            inicioX = e.pageX;
            scrollInicial = contenedor.scrollLeft;
        });

        window.addEventListener('mouseup', function () {
            arrastrando = false;
            contenedor.classList.remove('arrastrando');
        });

        window.addEventListener('mousemove', function (e) {
            if (!arrastrando) return;
            e.preventDefault();
            const distancia = e.pageX - inicioX;
            contenedor.scrollLeft = scrollInicial - distancia;
        });

        // Soporte táctil (celular/tablet) además del arrastre con mouse.
        contenedor.addEventListener('touchstart', function (e) {
            arrastrando = true;
            inicioX = e.touches[0].pageX;
            scrollInicial = contenedor.scrollLeft;
        }, { passive: true });

        contenedor.addEventListener('touchend', function () {
            arrastrando = false;
        });

        contenedor.addEventListener('touchmove', function (e) {
            if (!arrastrando) return;
            const distancia = e.touches[0].pageX - inicioX;
            contenedor.scrollLeft = scrollInicial - distancia;
        }, { passive: true });
    });
});
