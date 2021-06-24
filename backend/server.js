const express = require('express')
const { graphqlHTTP } = require('express-graphql')
const { buildSchema } = require('graphql')

const schema = buildSchema(`
    type LoadClient {
        id: String!
    }

    type Query {
        LoadClients: [LoadClient]!
    }
`)

const root = {
    LoadClients: () => {
        return [{ id: 'load-client-001' }, {id: 'load-client-002' }]
    }
}


const app = express()
app.use('/graphql', graphqlHTTP({
    schema,
    rootValue: root,
    graphiql: true
}))

const server = app.listen(8080, () => {
    console.log('Listening on port 8080...')
})

async function closeGracefully() {
    process.on('SIGTERM', () => {
        debug('SIGTERM signal received: closing HTTP server')
        server.close(() => {
            debug('HTTP server closed')
            process.exit()
        })
    })
}

process.on('SIGTERM', closeGracefully)
