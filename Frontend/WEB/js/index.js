const app = new Vue({
    el: '#app',
    data: {
        board: null,
    },
    computed: {
        table() {
            if (this.board == null) {
                return [];
            }
            let res = Array(this.board.gameBoardSize.width), i, j;
            for (i = 0; i < res.length; ++i) {
                res[i] = Array(this.board.gameBoardSize.height);
                for (j = 0; j < res[i].length; ++j) {
                    res[i][j] = 'none';
                }
            }
            res[this.board.snake[0].y][this.board.snake[0].x] = 'head';
            for (i = 1; i < this.board.snake.length; ++i)
                res[this.board.snake[i].y][this.board.snake[i].x] = 'tail';
            for (i = 0; i < this.board.food.length; ++i)
                res[this.board.food[i].y][this.board.food[i].x] = 'food';
            return res;
        }
    },
    methods: {
        getBoard() {
            axios.get('gameboard')
                .then(response => {
                    setTimeout(this.getBoard, response.data.timeUntilNextTurnMilliseconds);
                    this.board = response.data;
                }).catch(e => {
                    setTimeout(this.getBoard, 1000);
                });
        }
    },
    created() {
        // WARNING! Port could be different!
        axios.defaults.baseURL = 'https://localhost:44329/api/';
    },
    mounted() {
        window.addEventListener('keydown', e => {
            if (e.key.startsWith('Arrow')) {
                let direction = e.key.slice(5);
                if (direction === 'Up') {
                    direction = 'Top';
                } else if (direction === 'Down') {
                    direction = 'Bottom';
                }
                axios.post('direction', {
                    direction: direction,
                }).catch(e => {});
            }
        });
        this.getBoard();
    },
});
