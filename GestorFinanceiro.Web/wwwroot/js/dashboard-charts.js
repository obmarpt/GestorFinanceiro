(function () {
    var data = window.dashboardChartData;
    var canvas = document.getElementById('chartReceitasDespesas');
    var select = document.getElementById('chartSessaoSelect');

    if (!data || !canvas || !select || typeof Chart === 'undefined') {
        return;
    }

    var corReceitas = '#16A34A';
    var corDespesas = '#DC2626';
    var chart = null;

    function formatarEuro(v) {
        return v + ' €';
    }

    function obterConfig(sessaoId) {
        var labels, receitas, despesas, titulo;

        if (sessaoId === 'todas') {
            labels = data.sessoes.map(function (s) { return s.nome; });
            receitas = data.sessoes.map(function (s) { return s.receitas; });
            despesas = data.sessoes.map(function (s) { return s.despesas; });
            titulo = 'Todas as sessões';
        } else {
            var sessao = data.sessoes.find(function (s) {
                return String(s.id) === String(sessaoId);
            });
            if (!sessao) {
                return obterConfig('todas');
            }
            labels = ['Receitas', 'Despesas'];
            receitas = [sessao.receitas, 0];
            despesas = [0, sessao.despesas];
            titulo = sessao.nome;
        }

        return {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Receitas (€)',
                        data: receitas,
                        backgroundColor: corReceitas,
                        borderRadius: 6,
                        borderSkipped: false
                    },
                    {
                        label: 'Despesas (€)',
                        data: despesas,
                        backgroundColor: corDespesas,
                        borderRadius: 6,
                        borderSkipped: false
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                layout: {
                    padding: { top: 8, right: 12, bottom: 4, left: 8 }
                },
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { padding: 16, boxWidth: 14, font: { size: 12, weight: '600' } }
                    },
                    title: {
                        display: true,
                        text: titulo,
                        font: { size: 13, weight: '700' },
                        color: '#64748b',
                        padding: { bottom: 16 }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return formatarEuro(value);
                            }
                        },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        };
    }

    function atualizar() {
        var config = obterConfig(select.value);
        if (chart) {
            chart.destroy();
        }
        chart = new Chart(canvas, config);
    }

    select.addEventListener('change', atualizar);
    atualizar();
})();
